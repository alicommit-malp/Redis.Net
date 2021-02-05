using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Redis.Net.Contracts;
using Redis.Net.Options;
using Redis.Net.Services;
using StackExchange.Redis;
using Xunit;

namespace Redis.Net.Test
{
    public class LockTest
    {
        private readonly IRedisService _redisService;
        private static string KeyName() => Guid.NewGuid().ToString();

        public LockTest()
        {
            //docker run --name some-redis -d -p 6379:6379  redis redis-server --appendonly yes
            var configurationOptions = ConfigurationOptions.Parse("localhost:6379");
            configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
            var options = new RedisCacheOptions()
            {
                ConfigurationOptions = configurationOptions
            };
            var mock = new Mock<IOptionsMonitor<RedisCacheOptions>>();
            mock.Setup(m => m.Get(nameof(RedisDotNetTest))).Returns(options);
            _redisService =
                new RedisService<RedisDotNetTest>(mock.Object);
        }

        [Fact]
        public void LockTakeTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);
            _redisService.LockTakeString(keyName, keyValue, TimeSpan.FromSeconds(5));

            var result = _redisService.LockTakeString(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.False(result);

            Task.Delay(5100).GetAwaiter().GetResult();

            var result2 = _redisService.LockTakeString(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.True(result2);
            _redisService.Remove(keyName);
        }

        [Fact]
        public async Task LockTakeAsyncTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);
            await _redisService.LockTakeStringAsync(keyName, keyValue, TimeSpan.FromSeconds(5));

            var result = await _redisService.LockTakeStringAsync(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.False(result);

            await Task.Delay(5100);

            var result2 = await _redisService.LockTakeStringAsync(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.True(result2);
            await _redisService.RemoveAsync(keyName);
        }

        [Fact]
        public void LockReleaseTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);
            _redisService.LockTakeString(keyName, keyValue, TimeSpan.FromSeconds(5));

            var result = _redisService.LockTakeString(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.False(result);

            var result2 = _redisService.LockReleaseString(keyName, keyValue);
            Assert.True(result2);

            var result3 = _redisService.LockTakeString(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.True(result3);

            _redisService.Remove(keyName);
        }

        [Fact]
        public async Task LockReleaseAsyncTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);
            await _redisService.LockTakeStringAsync(keyName, keyValue, TimeSpan.FromSeconds(5));

            var result = await _redisService.LockTakeStringAsync(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.False(result);

            var result2 = await _redisService.LockReleaseStringAsync(keyName, keyValue);
            Assert.True(result2);

            var result3 = await _redisService.LockTakeStringAsync(keyName, keyValue, TimeSpan.FromSeconds(5));
            Assert.True(result3);

            await _redisService.RemoveAsync(keyName);
        }
    }
}