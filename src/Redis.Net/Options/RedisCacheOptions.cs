using Microsoft.Extensions.DependencyInjection;

namespace Redis.Net.Options
{
    /// <inheritdoc />
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RedisCacheOptions : Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions
    {
        /// <summary>
        /// <see cref="ServiceLifetime"/>
        /// </summary>
        public ServiceLifetime ServiceLifetimeScope { get; set; } = ServiceLifetime.Singleton;
        
        /// <summary>
        /// <see cref="CompressionOption"/>
        /// </summary>
        public CompressionOption CompressionOption { get; init; }
    }
}