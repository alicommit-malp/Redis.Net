using System;
using System.Collections.Concurrent;
using Redis.Net.Contracts;
using Redis.Net.Options;

namespace Redis.Net.Core
{
    internal static class RedisCacheInternalFactory
    {
        private static readonly ConcurrentDictionary<Type, RedisCacheInternal> RedisCacheInternals =
            new();

        public static IDistributedCacheInternal Get<TInstance>(RedisCacheOptions redisCacheOptions)
        {
            var result = RedisCacheInternals.GetOrAdd(typeof(TInstance),
                s => new RedisCacheInternal(redisCacheOptions));
            return result;
        }
    }
}