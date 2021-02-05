using System;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Redis.Net.Contracts;
using Redis.Net.Options;
using Redis.Net.Services;

namespace Redis.Net.Extensions
{
    /// <summary>
    ///  Redis cache lib, IOC service Extension 
    /// </summary>
    public static class ServiceExtension
    {
        /// <summary>
        /// Registers necessary service for the Redis.Net lib to the IOC engine
        /// <remarks>For working with multiple Redis servers</remarks>
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="redisCacheOptions"><see cref="RedisCacheOptions"/></param>
        /// <exception cref="ArgumentNullException">In case input parameters are null</exception>
        public static void AddRedisDotNet<TRedisService>(this IServiceCollection services,
            RedisCacheOptions redisCacheOptions)
            where TRedisService : RedisService<TRedisService>
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (redisCacheOptions == null)
            {
                throw new ArgumentNullException(nameof(RedisCacheOptions));
            }

            if (!string.IsNullOrEmpty(redisCacheOptions.Configuration) &&
                redisCacheOptions.ConfigurationOptions != null)
                throw new AmbiguousImplementationException(
                    $"Using both {nameof(RedisCacheOptions.Configuration)} " +
                    $"and {nameof(RedisCacheOptions.ConfigurationOptions)} at the same time is not allowed!");

            var redisOptionAction = new Action<RedisCacheOptions>(options =>
            {
                options.InstanceName = redisCacheOptions.InstanceName;
                options.ConfigurationOptions = redisCacheOptions.ConfigurationOptions;
                options.Configuration = redisCacheOptions.Configuration;
            });

            services.AddOptions();
            services.Configure(typeof(TRedisService).Name, redisOptionAction);

            CheckContextConstructors<TRedisService>();

            services.TryAdd(new ServiceDescriptor(typeof(TRedisService), typeof(TRedisService),
                redisCacheOptions.ServiceLifetimeScope));
        }

        /// <summary>
        /// Registers necessary service for the Redis.Net lib to the IOC engine
        /// <remarks>For working with single Redis server</remarks>
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="redisCacheOptions"><see cref="RedisCacheOptions"/></param>
        /// <exception cref="ArgumentNullException">In case input parameters are null</exception>
        public static void AddRedisDotNet(this IServiceCollection services,
            RedisCacheOptions redisCacheOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (redisCacheOptions == null)
            {
                throw new ArgumentNullException(nameof(RedisCacheOptions));
            }

            if (!string.IsNullOrEmpty(redisCacheOptions.Configuration) &&
                redisCacheOptions.ConfigurationOptions != null)
                throw new AmbiguousImplementationException(
                    $"Using both {nameof(RedisCacheOptions.Configuration)} " +
                    $"and {nameof(RedisCacheOptions.ConfigurationOptions)} at the same time is not allowed!");

            var adessoRedisOptionAction = new Action<RedisCacheOptions>(options =>
            {
                options.InstanceName = redisCacheOptions.InstanceName;
                options.ConfigurationOptions = redisCacheOptions.ConfigurationOptions;
                options.Configuration = redisCacheOptions.Configuration;
            });

            services.AddOptions();
            services.Configure(nameof(DefaultRedis), adessoRedisOptionAction);

            services.TryAdd(new ServiceDescriptor(typeof(IRedisService), typeof(RedisService<DefaultRedis>),
                redisCacheOptions.ServiceLifetimeScope));
        }

        /// <summary>
        /// A bridge class to support the old implementation  
        /// </summary>
        // ReSharper disable once ClassNeverInstantiated.Global
        internal class DefaultRedis : RedisService<DefaultRedis>
        {
            /// <summary>
            /// Calling the <see cref="ServiceExtension.AddRedisDotNet{TRedisService}"/> 
            /// </summary>
            /// <param name="cacheOptions"></param>
            public DefaultRedis(IOptionsMonitor<RedisCacheOptions> cacheOptions) : base(cacheOptions)
            {
            }
        }

        // ReSharper disable once UnusedTypeParameter
        private static void CheckContextConstructors<TRedisService>()
            where TRedisService : IRedisService
        {
            var declaredConstructors = typeof(IRedisService).GetTypeInfo().DeclaredConstructors.ToList();
            if (declaredConstructors.Count == 1
                && declaredConstructors[0].GetParameters().Length == 0)
            {
                throw new ArgumentException($"Missing constructor for {nameof(IRedisService)} implementation");
            }
        }
    }
}