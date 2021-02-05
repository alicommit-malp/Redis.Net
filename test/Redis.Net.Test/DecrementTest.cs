using System;
using System.Globalization;
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
    public class DecrementTest
    {
        private readonly IRedisService _redisService;
        private static string KeyName() => Guid.NewGuid().ToString();

        public DecrementTest()
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
        public async Task DecrGetRedisAsyncTest()
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            var result = await _redisService.DecrementStringAsync(keyName, 1L);
            await _redisService.RemoveAsync(keyName);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void DecrGetRedisTest()
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);
            var result = _redisService.DecrementString(keyName, 1L);
            _redisService.Remove(keyName);

            Assert.Equal(-1, result);
        }
        [Theory]
        [InlineData(1.5f, 0.25f, 1.25f)]
        [InlineData(0f, 0.25f, -0.25f)]
        [InlineData(-1.5f, 0.25f, -1.75f)]
        [InlineData(-1, 2, -3)]
        [InlineData(10, 2, 8)]
        public void Decrement(float first, float second, float expected)
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, Convert.ToString(first, CultureInfo.InvariantCulture));
            var result = _redisService.DecrementString(keyName, second);

            _redisService.Remove(keyName);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1.5f, 0.25f, 1.25f, 2000)]
        [InlineData(0f, 0.25f, -0.25f, 1000)]
        [InlineData(-1.5f, 0.25f, -1.75f, 1300)]
        [InlineData(-1, 2, -3, 2000)]
        [InlineData(10, 2, 8, 100)]
        public void DecrementTestWithExpiry(float first, float second, float expected, int milliSecond)
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, Convert.ToString(first, CultureInfo.InvariantCulture),
                TimeSpan.FromMilliseconds(milliSecond));
            var result = _redisService.DecrementString(keyName, second);
            Assert.Equal(expected, result);

            Task.Delay(milliSecond + 1000).GetAwaiter().GetResult();
            var resultAfterDelay = _redisService.Get<string>(keyName);
            Assert.Null(resultAfterDelay);

            _redisService.Remove(keyName);
        }


        [Theory]
        [InlineData(1.5f, 0.25f, 1.25f)]
        [InlineData(0f, 0.25f, -0.25f)]
        [InlineData(-1.5f, 0.25f, -1.75f)]
        [InlineData(-1, 2, -3)]
        [InlineData(10, 2, 8)]
        public async Task DecrementAsyncTest(float first, float second, float expected)
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, Convert.ToString(first, CultureInfo.InvariantCulture));
            var result = await _redisService.DecrementStringAsync(keyName, second);
            await _redisService.RemoveAsync(keyName);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1.5f, 0.25f, 1.25f, 2000)]
        [InlineData(0f, 0.25f, -0.25f, 1000)]
        [InlineData(-1.5f, 0.25f, -1.75f, 1300)]
        [InlineData(-1, 2, -3, 2000)]
        [InlineData(10, 2, 8, 100)]
        public async Task DecrementAsyncTestWithExpiry(float first, float second, float expected, int milliSecond)
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, Convert.ToString(first, CultureInfo.InvariantCulture),
                TimeSpan.FromMilliseconds(milliSecond));
            var result = await _redisService.DecrementStringAsync(keyName, second);
            Assert.Equal(expected, result);

            Task.Delay(milliSecond + 1000).GetAwaiter().GetResult();
            var resultAfterDelay = _redisService.Get<string>(keyName);
            Assert.Null(resultAfterDelay);

            await _redisService.RemoveAsync(keyName);
        }
    }
}