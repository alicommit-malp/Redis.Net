using System;
using System.Threading;
using System.Threading.Tasks;
using Redis.Net.Options;

namespace Redis.Net.Contracts
{
    /// <summary>
    /// Redis Cache library main contact <see cref="IDistributedCacheString"/>
    /// </summary>
    public interface IRedisService : IDistributedCacheString
    {
        /// <summary>
        /// Gets a value with the given key.
        /// <remarks>Which has been set with <see cref="IDistributedCacheString.SetStringAsync"/>
        /// or <see cref="IDistributedCacheString.SetString"/>
        /// </remarks>
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <returns>The located value or null.</returns>
        public T GetString<T>(string key);

        /// <summary>
        /// Gets a value with the given key.
        /// <remarks>Which has been set with <see cref="IDistributedCacheString.SetStringAsync"/>
        /// or <see cref="IDistributedCacheString.SetString"/>
        /// </remarks>
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the located value or null.</returns>
        public Task<T> GetStringAsync<T>(string key, CancellationToken token = new CancellationToken());
        
        /// <summary>
        /// Gets a value with the given key.
        /// <remarks>Which has been set with <see cref="SetAsync{T}"/> or <see cref="Set{T}"/></remarks>
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <returns>The located value or null.</returns>
        public T Get<T>(string key);

        /// <summary>
        /// Gets a value with the given key.
        /// <remarks>Which has been set with <see cref="SetAsync{T}"/> or <see cref="Set{T}"/></remarks>
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the located value or null.</returns>
        public Task<T> GetAsync<T>(string key, CancellationToken token = new CancellationToken());


        /// <summary>
        /// Sets a value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="options">The cache options for the value.<see cref="CacheOptions"/></param>
        public void Set<T>(string key, T value, CacheOptions options=null);

        /// <summary>
        /// Sets the value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="options">The cache options for the value <see cref="CacheOptions"/>.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SetAsync<T>(string key, T value, CacheOptions options=null,
            CancellationToken token = new CancellationToken());

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
        /// </summary>
        /// <param name="key">A string identifying the requested calue.</param>
        public void Refresh(string key);

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Removes the value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        public void Remove(string key);

        /// <summary>
        /// Removes the value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Expire a <paramref name="key"/> by a given <paramref name="timeSpan"/>
        /// </summary>
        /// <param name="key">string key</param>
        /// <param name="timeSpan">time span which the key should be expired <seealso cref="TimeSpan"/></param>
        /// <returns>true if the operation was successful</returns>
        public bool ExpireKey(string key,TimeSpan timeSpan);

        /// <summary>
        /// Asynchronously Expire a <paramref name="key"/> by a given <paramref name="timeSpan"/>
        /// </summary>
        /// <param name="key">string key</param>
        /// <param name="timeSpan">time span which the key should be expired <seealso cref="TimeSpan"/></param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns>true if the operation was successful</returns>
        public Task<bool> ExpireKeyAsync(string key,TimeSpan timeSpan,CancellationToken token= new CancellationToken());
        
        /// <summary>
        /// Is <paramref name="key"/> exist
        /// </summary>
        /// <param name="key">string key</param>
        /// <returns>true if the operation was successful</returns>
        public bool IsKeyExist(string key);

        /// <summary>
        /// Asynchronously check if the <paramref name="key"/> exists
        /// </summary>
        /// <param name="key">string key</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns>true if the operation was successful</returns>
        public Task<bool> IsKeyExistAsync(string key,CancellationToken token= new CancellationToken());
    }
}