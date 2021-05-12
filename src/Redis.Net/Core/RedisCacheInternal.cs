using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Redis.Net.Contracts;
using Redis.Net.Extensions;
using Redis.Net.Options;
using StackExchange.Redis;

namespace Redis.Net.Core
{
    /// <summary>
    /// Implementation of the Redis.Net Cache with StackExchange lib
    /// </summary>
    internal class RedisCacheInternal : IDistributedCacheInternal
    {
        private volatile ConnectionMultiplexer _connection;
        private IDatabase _cache;

        private readonly RedisCacheOptions _options;
        private readonly string _instance;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheOptions"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RedisCacheInternal(RedisCacheOptions cacheOptions)
        {
            _options = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));

            // This allows partitioning a single backend cache for use with multiple apps/services.
            _instance = _options.InstanceName ?? string.Empty;
        }

        /// <inheritdoc />
        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetAndRefresh(key, getData: true);
        }

        /// <inheritdoc />
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, getData: true, token: token);
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value, CacheOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Connect();

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            _cache.ScriptEvaluate(LunaConstants.SetScript, new RedisKey[] {_instance + key},
                new RedisValue[]
                {
                    absoluteExpiration?.Ticks ?? LunaConstants.NotPresent,
                    options.SlidingExpiration?.Ticks ?? LunaConstants.NotPresent,
                    GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? LunaConstants.NotPresent,
                    value
                });
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, byte[] value, CacheOptions options,
            CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token);

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            await _cache.ScriptEvaluateAsync(LunaConstants.SetScript, new RedisKey[] {_instance + key},
                new RedisValue[]
                {
                    absoluteExpiration?.Ticks ?? LunaConstants.NotPresent,
                    options.SlidingExpiration?.Ticks ?? LunaConstants.NotPresent,
                    GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? LunaConstants.NotPresent,
                    value
                });
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            GetAndRefresh(key, getData: false);
        }

        /// <inheritdoc />
        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, getData: false, token: token);
        }

        private void Connect()
        {
            if (_cache != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_cache != null) return;
                _connection = _options.ConfigurationOptions != null
                    ? ConnectionMultiplexer.Connect(_options.ConfigurationOptions)
                    : ConnectionMultiplexer.Connect(_options.Configuration);

                _cache = _connection.GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ConnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (_cache != null)
            {
                return;
            }

            await _connectionLock.WaitAsync(token);
            try
            {
                if (_cache == null)
                {
                    if (_options.ConfigurationOptions != null)
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions);
                    }
                    else
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration);
                    }

                    _cache = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private byte[] GetAndRefresh(string key, bool getData)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            var results = getData
                ? _cache.HashMemberGet(_instance + key, LunaConstants.AbsoluteExpirationKey,
                    LunaConstants.SlidingExpirationKey, LunaConstants.DataKey)
                : _cache.HashMemberGet(_instance + key, LunaConstants.AbsoluteExpirationKey,
                    LunaConstants.SlidingExpirationKey);

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out var absExpr, out var sldExpr);
                Refresh(key, absExpr, sldExpr);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        private async Task<byte[]> GetAndRefreshAsync(string key, bool getData,
            CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token);

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            if (getData)
            {
                results = await _cache.HashMemberGetAsync(_instance + key, LunaConstants.AbsoluteExpirationKey,
                    LunaConstants.SlidingExpirationKey,
                    LunaConstants.DataKey);
            }
            else
            {
                results = await _cache.HashMemberGetAsync(_instance + key, LunaConstants.AbsoluteExpirationKey,
                    LunaConstants.SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out var absExpr, out var sldExpr);
                await RefreshAsync(key, absExpr, sldExpr, token);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            _cache.KeyDelete(_instance + key);
            // TODO: Error handling
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            await _cache.KeyDeleteAsync(_instance + key);
            // TODO: Error handling
        }

        /// <inheritdoc />
        public bool ExpireKey(string key, TimeSpan timeSpan)
        {
            Connect();
            return _cache.KeyExpire(_instance + key, timeSpan);
        }

        /// <inheritdoc />
        public async Task<bool> ExpireKeyAsync(string key, TimeSpan timeSpan,
            CancellationToken token = new CancellationToken())
        {
            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.KeyExpireAsync(_instance + key, timeSpan);
        }

        /// <inheritdoc />
        public bool IsKeyExist(string key)
        {
            Connect();
            return _cache.KeyExists(key);
        }

        /// <inheritdoc />
        public async Task<bool> IsKeyExistAsync(string key, CancellationToken token = new CancellationToken())
        {
            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.KeyExistsAsync(key);
        }
        
        
        public async Task<IEnumerable<string>> GetKeysAsync(string pattern,
            CancellationToken token = new CancellationToken())
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            var result = await _cache.ScriptEvaluateAsync(LunaConstants.GetKeysByPatternScript,
                null,
                new RedisValue[]
                {
                    pattern,
                });

            return (string[]) result;
        }

        public IEnumerable<string> GetKeys(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            Connect();

            var result = _cache.ScriptEvaluate(LunaConstants.GetKeysByPatternScript,
                null,
                new RedisValue[]
                {
                    pattern,
                });

            return (string[]) result;
        }

        public async Task RemoveKeysAsync(string pattern, CancellationToken token = new CancellationToken())
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            await _cache.ScriptEvaluateAsync(LunaConstants.DelKeysByPatternScript,
                null,
                new RedisValue[]
                {
                    pattern,
                });
        }

        public void RemoveKeys(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            Connect();

            _cache.ScriptEvaluate(LunaConstants.DelKeysByPatternScript,
                null,
                new RedisValue[]
                {
                    pattern,
                });
        }

        public async Task<long> GetKeysCountAsync(string pattern, CancellationToken token = new CancellationToken())
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            var result = await _cache.ScriptEvaluateAsync(LunaConstants.GetKeysCountByPatternScript,
                null,
                new RedisValue[]
                {
                    pattern,
                });

            return (long) result;
        }

        public long GetKeysCount(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            Connect();

            var result = _cache.ScriptEvaluate(LunaConstants.GetKeysCountByPatternScript,
                null,
                new RedisValue[]
                {
                    pattern,
                });

            return (long) result;
        }

        private static void MapMetadata(IReadOnlyList<RedisValue> results, out DateTimeOffset? absoluteExpiration,
            out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?) results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != LunaConstants.NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }

            var slidingExpirationTicks = (long?) results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != LunaConstants.NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (!sldExpr.HasValue) return;
            TimeSpan? expr;
            if (absExpr.HasValue)
            {
                var relExpr = absExpr.Value - DateTimeOffset.Now;
                expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
            }
            else
            {
                expr = sldExpr;
            }

            _cache.KeyExpire(_instance + key, expr);
            // TODO: Error handling
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr,
            CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (sldExpr.HasValue)
            {
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }

                await _cache.KeyExpireAsync(_instance + key, expr);
                // TODO: Error handling
            }
        }

        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration,
            CacheOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long) Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }

            if (absoluteExpiration.HasValue)
            {
                return (long) (absoluteExpiration.Value - creationTime).TotalSeconds;
            }

            if (options.SlidingExpiration.HasValue)
            {
                return (long) options.SlidingExpiration.Value.TotalSeconds;
            }

            return null;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime,
            CacheOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(CacheOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }

            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _connection?.Close();
        }

        /// <inheritdoc />
        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? timeSpan,
            CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.StringSetAsync(_instance + key, value, timeSpan);
        }

        /// <inheritdoc />
        public bool SetString(string key, string value, TimeSpan? timeSpan)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();
            return _cache.StringSet(_instance + key, value, timeSpan);
        }

        /// <inheritdoc />
        public async Task<long> IncrementStringAsync(string key, long gradient = 1L,
            CancellationToken token = new CancellationToken())
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.StringIncrementAsync(_instance + key, gradient);
        }

        public async Task<long> IncrementStringAsync(string key, TimeSpan timeSpan, double gradient = 1,
            CancellationToken token = new CancellationToken())
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            var result = await _cache.ScriptEvaluateAsync(LunaConstants.IncrementWithTtlScript,
                new RedisKey[] {_instance + key},
                new RedisValue[]
                {
                    Convert.ToInt64(timeSpan.TotalSeconds),
                    gradient,
                });

            return (long) result;
        }

        public async Task<long> IncrementStringAsync(string key, TimeSpan timeSpan, long gradient = 1,
            CancellationToken token = new CancellationToken())
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            var result = await _cache.ScriptEvaluateAsync(LunaConstants.IncrementWithTtlScript,
                new RedisKey[] {_instance + key},
                new RedisValue[]
                {
                    Convert.ToInt64(timeSpan.TotalSeconds),
                    gradient,
                });

            return (long) result;
        }

        /// <inheritdoc />
        public long IncrementString(string key, long gradient = 1)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();
            return _cache.StringIncrement(_instance + key, gradient);
        }

        public long IncrementString(string key, TimeSpan timeSpan, long gradient = 1)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            var result = _cache.ScriptEvaluate(LunaConstants.IncrementWithTtlScript,
                new RedisKey[] {_instance + key},
                new RedisValue[]
                {
                    Convert.ToInt64(timeSpan.TotalSeconds),
                    gradient,
                });

            return (long) result;
        }

        /// <inheritdoc />
        public async Task<double> IncrementStringAsync(string key, double gradient = 1D,
            CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.StringIncrementAsync(_instance + key, gradient);
        }

        /// <inheritdoc />
        public double IncrementString(string key, double gradient = 1)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();
            return _cache.StringIncrement(_instance + key, gradient);
        }

        /// <inheritdoc />
        public double IncrementString(string key, TimeSpan timeSpan, double gradient = 1)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            var result = _cache.ScriptEvaluate(LunaConstants.IncrementWithTtlScript,
                new RedisKey[] {_instance + key},
                new RedisValue[]
                {
                    Convert.ToInt64(timeSpan.TotalSeconds),
                    gradient,
                });

            return (long) result;
        }

        /// <inheritdoc />
        public async Task<long> DecrementStringAsync(string key, long gradient = 1L, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.StringDecrementAsync(_instance + key, gradient);
        }

        /// <inheritdoc />
        public long DecrementString(string key, long gradient = 1)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();
            return _cache.StringDecrement(_instance + key, gradient);
        }

        /// <inheritdoc />
        public async Task<double> DecrementStringAsync(string key, double gradient = 1D,
            CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.StringDecrementAsync(_instance + key, gradient);
        }

        /// <inheritdoc />
        public double DecrementString(string key, double gradient = 1)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();
            return _cache.StringDecrement(_instance + key, gradient);
        }

        /// <inheritdoc />
        public bool LockTakeString(string key, string value, TimeSpan timeSpan)
        {
            Connect();
            return _cache.LockTake(_instance + key, value, timeSpan);
        }

        /// <inheritdoc />
        public async Task<bool> LockTakeStringAsync(string key, string value, TimeSpan timeSpan,
            CancellationToken token = new CancellationToken())
        {
            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.LockTakeAsync(_instance + key, value, timeSpan);
        }

        /// <inheritdoc />
        public bool LockReleaseString(string key, string value)
        {
            Connect();
            return _cache.LockRelease(_instance + key, value);
        }

        /// <inheritdoc />
        public async Task<bool> LockReleaseStringAsync(string key, string value,
            CancellationToken token = new CancellationToken())
        {
            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            return await _cache.LockReleaseAsync(_instance + key, value);
        }


        /// <inheritdoc />
        public string GetString(string key)
        {
            Connect();
            var result = _cache.StringGet(_instance + key);
            
            if (result.IsNull) return null;
            return result;
        }

        /// <inheritdoc />
        public async Task<string> GetStringAsync(string key, CancellationToken token = new CancellationToken())
        {
            await ConnectAsync(token);
            token.ThrowIfCancellationRequested();

            var result =await _cache.StringGetAsync(key);
            
            if (result.IsNull) return null;
            return result;
        }
    }
}