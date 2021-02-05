using System;
using System.Threading;
using System.Threading.Tasks;

namespace Redis.Net.Contracts
{
    /// <summary>
    /// Extra functionality beside the <see cref="IDistributedCacheInternal"/>  
    /// </summary>
    public interface IDistributedCacheString
    {
        /// <summary>
        /// Gets a value with the given key.
        /// <remarks>Which has been set with <see cref="IDistributedCacheString.SetStringAsync"/>
        /// or <see cref="IDistributedCacheString.SetString"/>
        /// </remarks>
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <returns>The located value or null.</returns>
        public string GetString(string key);

        /// <summary>
        /// Gets a value with the given key.
        /// <remarks>Which has been set with <see cref="IDistributedCacheString.SetStringAsync"/>
        /// or <see cref="IDistributedCacheString.SetString"/>
        /// </remarks>
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the located value or null.</returns>
        public Task<string> GetStringAsync(string key, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Asynchronously Set the value of the <paramref name="key"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="value">a string value can be int , string , float , double. </param>
        /// <param name="timeSpan"><see cref="TimeSpan"/> the expiry time relative to now.</param>
        /// <param name="token"></param>
        /// <returns>true if the operations is successful.</returns>
        Task<bool> SetStringAsync(string key, string value, TimeSpan? timeSpan = null,
            CancellationToken token = new CancellationToken());

        /// <summary>
        /// Set the value of the <paramref name="key"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="value">a string value can be int , string , float , double. </param>
        /// <param name="timeSpan"><see cref="TimeSpan"/> the expiry time relative to now.</param>
        /// <returns>true if the operations is successful.</returns>
        bool SetString(string key, string value, TimeSpan? timeSpan = null);

        /// <summary>
        /// Asynchronously Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in long</param>
        /// <param name="token"></param>
        /// <returns>long value of the <paramref name="key"/> after the increment.</returns>
        Task<long> IncrementStringAsync(string key, long gradient = 1L,
            CancellationToken token = new CancellationToken());

        /// <summary>
        /// Asynchronously Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in long</param>
        /// <param name="timeSpan">set TTL if the key did not exist</param>
        /// <param name="token"></param>
        /// <remarks>In case the key does not exist, the incr will create the key with TTL</remarks>
        /// <returns>long value of the <paramref name="key"/> after the increment.</returns>
        Task<long> IncrementStringAsync(string key,TimeSpan timeSpan, double gradient = 1D,
            CancellationToken token = new CancellationToken());
        
        /// <summary>
        /// Asynchronously Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in long</param>
        /// <param name="timeSpan">set TTL if the key did not exist</param>
        /// <param name="token"></param>
        /// <remarks>In case the key does not exist, the incr will create the key with TTL</remarks>
        /// <returns>long value of the <paramref name="key"/> after the increment.</returns>
        Task<long> IncrementStringAsync(string key,TimeSpan timeSpan, long gradient = 1L,
            CancellationToken token = new CancellationToken());

        /// <summary>
        /// Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in long</param>
        /// <returns>long value of the <paramref name="key"/> after the increment.</returns>
        long IncrementString(string key, long gradient = 1L);

        /// <summary>
        /// Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="timeSpan">set TTL if the key did not exist</param>
        /// <param name="gradient">the gradient of the change in long</param>
        /// <remarks>In case the key does not exist, the incr will create the key with TTL</remarks>
        /// <returns>long value of the <paramref name="key"/> after the increment.</returns>
        long IncrementString(string key,TimeSpan timeSpan ,long gradient = 1L);
        
        /// <summary>
        /// Asynchronously Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in double</param>
        /// <param name="token"></param>
        /// <returns>double value of the <paramref name="key"/> after the increment.</returns>
        Task<double> IncrementStringAsync(string key, double gradient = 1D,
            CancellationToken token = new CancellationToken());

        /// <summary>
        /// Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in double</param>
        /// <returns>double value of the <paramref name="key"/> after the increment.</returns>
        double IncrementString(string key, double gradient = 1D);
        
        
        /// <summary>
        /// Increment the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="timeSpan">set TTL if the key did not exist</param>
        /// <param name="gradient">the gradient of the change in double</param>
        /// <remarks>In case the key does not exist, the incr will create the key with TTL</remarks>
        /// <returns>double value of the <paramref name="key"/> after the increment.</returns>
        double IncrementString(string key,TimeSpan timeSpan, double gradient = 1D);

        /// <summary>
        /// Asynchronously decrement the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in long</param>
        /// <param name="token"></param>
        /// <returns>long value of the <paramref name="key"/> after the decrement.</returns>
        Task<long> DecrementStringAsync(string key, long gradient = 1L,
            CancellationToken token = new CancellationToken());

        /// <summary>
        /// Decrement the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in long</param>
        /// <returns>long value of the <paramref name="key"/> after the decrement.</returns>
        long DecrementString(string key, long gradient = 1L);


        /// <summary>
        /// Asynchronously decrement the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in double</param>
        /// <param name="token"></param>
        /// <returns>double value of the <paramref name="key"/> after the decrement.</returns>
        Task<double> DecrementStringAsync(string key, double gradient = 1D,
            CancellationToken token = new CancellationToken());


        /// <summary>
        /// Decrement the value of the <paramref name="key"/> by <paramref name="gradient"/>.
        /// </summary>
        /// <param name="key">The key in redis cache.</param>
        /// <param name="gradient">the gradient of the change in double</param>
        /// <returns>double value of the <paramref name="key"/> after the decrement.</returns>
        double DecrementString(string key, double gradient = 1D);

        /// <summary>
        /// Takes a lock (specifying a token value) if it is not already taken.
        /// </summary>
        /// <param name="key">The key of the lock.</param>
        /// <param name="value">The value to set at the key.</param>
        /// <param name="timeSpan">The expiration of the lock key.</param>
        /// <returns>True if the lock was successfully taken, false otherwise.</returns>
        public bool LockTakeString(string key, string value, TimeSpan timeSpan);

        /// <summary>
        /// Takes a lock Asynchronously (specifying a token value) if it is not already taken.
        /// </summary>
        /// <param name="key">The key of the lock.</param>
        /// <param name="value">The value to set at the key.</param>
        /// <param name="timeSpan">The expiration of the lock key.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns>True if the lock was successfully taken, false otherwise.</returns>
        public Task<bool> LockTakeStringAsync(string key, string value, TimeSpan timeSpan,
            CancellationToken token = new CancellationToken());

        /// <summary>Releases a lock, if the token value is correct.</summary>
        /// <param name="key">The key of the lock.</param>
        /// <param name="value">The value at the key tht must match.</param>
        /// <returns>True if the lock was successfully released, false otherwise.</returns>
        public bool LockReleaseString(string key, string value);

        /// <summary>Releases a lock Asynchronously, if the token value is correct.</summary>
        /// <param name="key">The key of the lock.</param>
        /// <param name="value">The value at the key tht must match.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns>True if the lock was successfully released, false otherwise.</returns>
        public Task<bool> LockReleaseStringAsync(string key, string value,
            CancellationToken token = new CancellationToken());
    }
}