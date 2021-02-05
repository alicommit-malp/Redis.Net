using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Redis.Net.Contracts;
using Redis.Net.Core;
using Redis.Net.Options;

namespace Redis.Net.Services
{
    /// <inheritdoc />
    public class RedisService<TInstance> : IRedisService
    {
        private readonly IDistributedCacheInternal _distributedCacheInternal;
        private readonly ISerializer _serializer = new Serializer();

        /// <summary>
        ///  Redis service constructor 
        /// </summary>
        /// <param name="cacheOptions"><see cref="CacheOptions"/></param>
        public RedisService(IOptionsMonitor<RedisCacheOptions> cacheOptions)
        {
            _distributedCacheInternal =
                RedisCacheInternalFactory.Get<TInstance>(cacheOptions.Get(typeof(TInstance).Name));
        }

        /// <inheritdoc />
        public T GetString<T>(string key)
        {
            var result = _distributedCacheInternal.GetString(key);
            return _serializer.ConvertFromString<T>(result);
        }

        /// <inheritdoc />
        public async Task<T> GetStringAsync<T>(string key, CancellationToken token = new CancellationToken())
        {
            var result = await _distributedCacheInternal.GetStringAsync(key, token);
            return _serializer.ConvertFromString<T>(result);
        }

        /// <inheritdoc />
        public T Get<T>(string key)
        {
            var result = _distributedCacheInternal.Get(key);
            return _serializer.ConvertFromByte<T>(result);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key, CancellationToken token = new CancellationToken())
        {
            var result = await _distributedCacheInternal.GetAsync(key, token);
            return _serializer.ConvertFromByte<T>(result);
        }


        /// <inheritdoc />
        public void Set<T>(string key, T value, CacheOptions options = null)
        {
            options ??= new CacheOptions();
            _distributedCacheInternal.Set(key, _serializer.ConvertToByte(value), options);
        }

        /// <inheritdoc />
        public Task SetAsync<T>(string key, T value, CacheOptions options = null,
            CancellationToken token = new CancellationToken())
        {
            options ??= new CacheOptions();
            return _distributedCacheInternal.SetAsync(key, _serializer.ConvertToByte(value), options, token);
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            _distributedCacheInternal.Refresh(key);
        }

        /// <inheritdoc />
        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.RefreshAsync(key, token);
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            _distributedCacheInternal.Remove(key);
        }

        /// <inheritdoc />
        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.RemoveAsync(key, token);
        }

        /// <inheritdoc />
        public bool ExpireKey(string key, TimeSpan timeSpan)
        {
            return _distributedCacheInternal.ExpireKey(key, timeSpan);
        }

        /// <inheritdoc />
        public Task<bool> ExpireKeyAsync(string key, TimeSpan timeSpan,
            CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.ExpireKeyAsync(key, timeSpan, token);
        }

        /// <inheritdoc />
        public bool IsKeyExist(string key)
        {
            return _distributedCacheInternal.IsKeyExist(key);
        }

        /// <inheritdoc />
        public Task<bool> IsKeyExistAsync(string key, CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.IsKeyExistAsync(key, token);
        }


        /// <inheritdoc />
        public string GetString(string key)
        {
            return _distributedCacheInternal.GetString(key);
        }

        /// <inheritdoc />
        public Task<string> GetStringAsync(string key, CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.GetStringAsync(key, token);
        }

        /// <inheritdoc />
        public Task<bool> SetStringAsync(string key, string value, TimeSpan? timeSpan = null,
            CancellationToken token = default)
        {
            return _distributedCacheInternal.SetStringAsync(key, value, timeSpan, token);
        }

        /// <inheritdoc />
        public bool SetString(string key, string value, TimeSpan? timeSpan = null)
        {
            return _distributedCacheInternal.SetString(key, value, timeSpan);
        }

        /// <inheritdoc />
        public Task<long> IncrementStringAsync(string key, long gradient = 1L,
            CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.IncrementStringAsync(key, gradient, token);
        }

        /// <inheritdoc />
        public Task<long> IncrementStringAsync(string key, TimeSpan timeSpan, double gradient = 1,
            CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.IncrementStringAsync(key, timeSpan, gradient, token);
        }

        /// <inheritdoc />
        public Task<long> IncrementStringAsync(string key, TimeSpan timeSpan, long gradient = 1,
            CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.IncrementStringAsync(key, timeSpan, gradient, token);
        }

        /// <inheritdoc />
        public long IncrementString(string key, long gradient = 1)
        {
            return _distributedCacheInternal.IncrementString(key, gradient);
        }

        /// <inheritdoc />
        public long IncrementString(string key, TimeSpan timeSpan, long gradient = 1)
        {
            return _distributedCacheInternal.IncrementString(key, timeSpan, gradient);
        }

        /// <inheritdoc />
        public Task<double> IncrementStringAsync(string key, double gradient = 1D, CancellationToken token = default)
        {
            return _distributedCacheInternal.IncrementStringAsync(key, gradient, token);
        }

        /// <inheritdoc />
        public double IncrementString(string key, double gradient = 1)
        {
            return _distributedCacheInternal.IncrementString(key, gradient);
        }

        /// <inheritdoc />
        public double IncrementString(string key, TimeSpan timeSpan, double gradient = 1)
        {
            return _distributedCacheInternal.IncrementString(key, timeSpan, gradient);
        }

        /// <inheritdoc />
        public Task<long> DecrementStringAsync(string key, long gradient = 1L, CancellationToken token = default)
        {
            return _distributedCacheInternal.DecrementStringAsync(key, gradient, token);
        }

        /// <inheritdoc />
        public long DecrementString(string key, long gradient = 1)
        {
            return _distributedCacheInternal.DecrementString(key, gradient);
        }

        /// <inheritdoc />
        public Task<double> DecrementStringAsync(string key, double gradient = 1D, CancellationToken token = default)
        {
            return _distributedCacheInternal.DecrementStringAsync(key, gradient, token);
        }

        /// <inheritdoc />
        public double DecrementString(string key, double gradient = 1)
        {
            return _distributedCacheInternal.DecrementString(key, gradient);
        }

        /// <inheritdoc />
        public bool LockTakeString(string key, string value, TimeSpan timeSpan)
        {
            return _distributedCacheInternal.LockTakeString(key, value, timeSpan);
        }

        /// <inheritdoc />
        public Task<bool> LockTakeStringAsync(string key, string value, TimeSpan timeSpan,
            CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.LockTakeStringAsync(key, value, timeSpan, token);
        }

        /// <inheritdoc />
        public bool LockReleaseString(string key, string value)
        {
            return _distributedCacheInternal.LockReleaseString(key, value);
        }

        /// <inheritdoc />
        public Task<bool> LockReleaseStringAsync(string key, string value,
            CancellationToken token = new CancellationToken())
        {
            return _distributedCacheInternal.LockReleaseStringAsync(key, value, token);
        }
    }
}