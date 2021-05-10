namespace Redis.Net.Core
{
    /// <summary>
    /// Static repository of the Lua commands for Redis 
    /// </summary>
    internal static class LunaConstants
    {
        /// <summary>
        /// Lua Set script with expiration configurations 
        /// </summary>
        public const string SetScript = (@"
                redis.call('HMSET', KEYS[1], 'absexp', ARGV[1], 'sldexp', ARGV[2], 'data', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1");

        /// <summary>
        /// Lua script increment by <paramref>
        ///     <name>ARGV[2]</name>
        /// </paramref>
        /// and then check if the key has TTL if not it will
        /// set the TTL to <paramref>
        ///     <name>ARGV[1]</name>
        /// </paramref>
        /// </summary>
        public const string IncrementWithTtlScript = (@"
                local ret = redis.call('incrbyfloat', KEYS[1], ARGV[2])
                if ret == ARGV[2] then
                  redis.call('EXPIRE', KEYS[1] , ARGV[1])
                end 
                return ret");

        /// <summary>
        /// Lua script get all keys matches the given pattern
        /// </summary>
        public const string GetKeysByPatternScript = (@"return redis.call('KEYS', ARGV[1])");


        /// <summary>
        /// Lua script delete all keys matches the given pattern
        /// </summary>
        public const string DelKeysByPatternScript = (@"
            local keys = redis.call('KEYS', ARGV[1])
            for i,k in ipairs(keys) do
                redis.call('DEL', k)
            end
        ");

        public const string GetKeysCountByPatternScript = (@"
              local keys = redis.call('KEYS', ARGV[1])
              local count = 0
              for i,k in ipairs(keys) do
                  count = count + 1
              end
              return count
          ");

        /// <summary>
        /// Lua absolute expiration keyword 
        /// </summary>
        public const string AbsoluteExpirationKey = "absexp";

        /// <summary>
        /// Lua sliding expiration keyword 
        /// </summary>
        public const string SlidingExpirationKey = "sldexp";

        /// <summary>
        /// Lua data keyword 
        /// </summary>
        public const string DataKey = "data";

        /// <summary>
        /// Lua not presented value 
        /// </summary>
        public const long NotPresent = -1;
    }
}