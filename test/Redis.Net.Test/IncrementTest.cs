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
    public class IncrementTest
    {
        private readonly IRedisService _redisService;
        private static string KeyName() => Guid.NewGuid().ToString();

        public IncrementTest()
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
        public void IncrGetRedisTest()
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);
            var result = _redisService.IncrementString(keyName, 1D);
            _redisService.Remove(keyName);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task IncrGetRedisAsyncTest()
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            var result = await _redisService.IncrementStringAsync(keyName, 1D);
            await _redisService.RemoveAsync(keyName);

            Assert.Equal(1, result);
        }

        [Theory]
        [InlineData(1.5f, 0.25f, 1.75f)]
        [InlineData(0f, 0.25f, 0.25f)]
        [InlineData(-1.5f, 0.25f, -1.25f)]
        [InlineData(-1, 2, 1)]
        [InlineData(10, 2, 12)]
        public async Task IncrementAsyncTest(float first, float second, float expected)
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, Convert.ToString(first, CultureInfo.InvariantCulture));
            var result = await _redisService.IncrementStringAsync(keyName, second);
            await _redisService.RemoveAsync(keyName);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1.5f, 0.25f, 1.75f)]
        [InlineData(0f, 0.25f, 0.25f)]
        [InlineData(-1.5f, 0.25f, -1.25f)]
        [InlineData(-1, 2, 1)]
        [InlineData(10, 2, 12)]
        public void Increment(float first, float second, float expected)
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, Convert.ToString(first, CultureInfo.InvariantCulture));
            var result = _redisService.IncrementString(keyName, second);
            _redisService.Remove(keyName);

            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(1.5f, 0.25f, 1.75f, 2000)]
        [InlineData(0f, 0.25f, 0.25f, 3000)]
        [InlineData(-1.5f, 0.25f, -1.25f, 1000)]
        [InlineData(-1, 2, 1, 2000)]
        [InlineData(10, 2, 12, 1500)]
        public void IncrementTestWithExpiry(float first, float second, float expected, int milliSecond)
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, Convert.ToString(first, CultureInfo.InvariantCulture),
                TimeSpan.FromMilliseconds(milliSecond));
            var result = _redisService.IncrementString(keyName, second);
            Assert.Equal(expected, result);

            Task.Delay(milliSecond + 1000).GetAwaiter().GetResult();
            var resultAfterDelay = _redisService.Get<string>(keyName);
            Assert.Null(resultAfterDelay);

            _redisService.Remove(keyName);
        }

        [Theory]
        [InlineData(1.5f, 0.25f, 1.75f, 2000)]
        [InlineData(0f, 0.25f, 0.25f, 3000)]
        [InlineData(-1.5f, 0.25f, -1.25f, 1000)]
        [InlineData(-1, 2, 1, 2000)]
        [InlineData(10, 2, 12, 1500)]
        public async Task IncrementAsyncTestWithExpiry(double first, double second, double expected, int milliSecond)
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, Convert.ToString(first, CultureInfo.InvariantCulture),
                TimeSpan.FromMilliseconds(milliSecond));
            var result = await _redisService.IncrementStringAsync(keyName, second);
            Assert.Equal(expected, result);

            Task.Delay(milliSecond + 1000).GetAwaiter().GetResult();
            var resultAfterDelay = _redisService.Get<string>(keyName);
            Assert.Null(resultAfterDelay);

            await _redisService.RemoveAsync(keyName);
        }

        [Theory]
        [InlineData(1, 2, 3, 2000)]
        [InlineData(2, -2, 0, 2000)]
        public async Task IncrementAsyncTestWithExpiryLong(long first, long second, long expected, int milliSecond)
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, Convert.ToString(first, CultureInfo.InvariantCulture),
                TimeSpan.FromMilliseconds(milliSecond));
            var result = await _redisService.IncrementStringAsync(keyName, second);
            Assert.Equal(expected, result);

            Task.Delay(milliSecond + 1000).GetAwaiter().GetResult();
            var resultAfterDelay = _redisService.Get<string>(keyName);
            Assert.Null(resultAfterDelay);

            await _redisService.RemoveAsync(keyName);
        }

        [Fact]
        public async Task IncrementWithTtlAsync_NotExpiredKey()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);

            var ttl = TimeSpan.FromSeconds(30);
            const double addition = 2.0;

            //add a key with some seconds TTL
            await _redisService.SetStringAsync(keyName, keyValue, ttl);

            await _redisService.IncrementStringAsync(keyName, ttl, addition);

            var newValue = await _redisService.GetStringAsync(keyName);

            Assert.Equal(addition + Convert.ToDouble(keyValue), Convert.ToDouble(newValue));

            await Task.Delay((int) (ttl.TotalMilliseconds + 200));

            var newIncrementedValue = await _redisService.GetStringAsync(keyName);

            //at this point the incremented ket must be expired again 
            Assert.Null(newIncrementedValue);
        }

        [Fact]
        public void IncrementString()
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);

            const long addition = 2;

            //at this point the key must be expired 
            _redisService.IncrementString(keyName, addition);
            var newValue = _redisService.GetString(keyName);

            Assert.NotNull(newValue);
            _redisService.Remove(keyName);
        }

        [Fact]
        public void IncrementWithTtl()
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);

            var ttl = TimeSpan.FromSeconds(3);
            const double addition = 2.0;

            //at this point the key must be expired 
            _redisService.IncrementString(keyName, ttl, addition);

            Task.Delay((int) (ttl.TotalMilliseconds + 200)).GetAwaiter().GetResult();

            var newValue = _redisService.GetString(keyName);

            Assert.Null(newValue);
        }

        [Fact]
        public void IncrementWithDoubleTtl()
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);

            var ttl = TimeSpan.FromSeconds(3.5);
            const double addition = 2.0;

            //at this point the key must be expired 
            _redisService.IncrementString(keyName, ttl, addition);

            Task.Delay((int) (ttl.TotalMilliseconds + 1000)).GetAwaiter().GetResult();

            var newValue = _redisService.GetString(keyName);

            Assert.Null(newValue);
        }

        [Fact]
        public void IncrementWithTtl_Long()
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);

            var ttl = TimeSpan.FromSeconds(3);
            const long addition = 2;

            //at this point the key must be expired 
            _redisService.IncrementString(keyName, ttl, addition);

            Task.Delay((int) (ttl.TotalMilliseconds + 200)).GetAwaiter().GetResult();

            var newValue = _redisService.GetString(keyName);

            Assert.Null(newValue);
        }

        [Fact]
        public async Task IncrementWithDoubleTtlAsync_Long()
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);

            var ttl = TimeSpan.FromSeconds(3.5);
            const long addition = 2;

            //at this point the key must be expired 
            await _redisService.IncrementStringAsync(keyName, ttl, addition);

            Task.Delay((int) (ttl.TotalMilliseconds + 1000)).GetAwaiter().GetResult();

            var newValue = await _redisService.GetStringAsync(keyName);

            Assert.Null(newValue);
        }

        [Fact]
        public async Task IncrementWithTtlAsync_Long()
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);

            var ttl = TimeSpan.FromSeconds(3);
            const long addition = 2;

            //at this point the key must be expired 
            await _redisService.IncrementStringAsync(keyName, ttl, addition);

            Task.Delay((int) (ttl.TotalMilliseconds + 200)).GetAwaiter().GetResult();

            var newValue = await _redisService.GetStringAsync(keyName);

            Assert.Null(newValue);
        }

        [Fact]
        public void IncrementWithTtl_ExpiredKey()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);

            var ttl = TimeSpan.FromSeconds(3);
            const double addition = 2.0;

            //add a key with some seconds TTL
            _redisService.SetString(keyName, keyValue, ttl);

            Task.Delay((int) (ttl.TotalMilliseconds + 200)).GetAwaiter().GetResult();

            //at this point the key must be expired 
            _redisService.IncrementString(keyName, ttl, addition);

            var newValue = _redisService.GetString(keyName);

            Assert.Equal(addition, Convert.ToDouble(newValue));

            Task.Delay((int) (ttl.TotalMilliseconds + 200)).GetAwaiter().GetResult();

            var newIncrementedValue = _redisService.GetString(keyName);

            //at this point the incremented ket must be expired again 
            Assert.Null(newIncrementedValue);
        }

        [Fact]
        public void IncrementWithDoubleTtl_ExpiredKey()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);

            var ttl = TimeSpan.FromSeconds(3.5);
            const double addition = 2.0;

            //add a key with some seconds TTL
            _redisService.SetString(keyName, keyValue, ttl);

            Task.Delay((int) (ttl.TotalMilliseconds + 1000)).GetAwaiter().GetResult();

            //at this point the key must be expired 
            _redisService.IncrementString(keyName, ttl, addition);

            var newValue = _redisService.GetString(keyName);

            Assert.Equal(addition, Convert.ToDouble(newValue));

            Task.Delay((int) (ttl.TotalMilliseconds + 1000)).GetAwaiter().GetResult();

            var newIncrementedValue = _redisService.GetString(keyName);

            //at this point the incremented ket must be expired again 
            Assert.Null(newIncrementedValue);
        }

        [Fact]
        public void IncrementWithDoubleTtl_NotExpiredKey()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);

            var ttl = TimeSpan.FromSeconds(3.5);
            const double addition = 2.0;

            //add a key with some seconds TTL
            _redisService.SetString(keyName, keyValue, ttl);

            _redisService.IncrementString(keyName, ttl, addition);

            var newValue = _redisService.GetString(keyName);

            Assert.Equal(addition + Convert.ToDouble(keyValue), Convert.ToDouble(newValue));

            Task.Delay((int) (ttl.TotalMilliseconds + 1000)).GetAwaiter().GetResult();

            var newIncrementedValue = _redisService.GetString(keyName);

            //at this point the incremented ket must be expired again 
            Assert.Null(newIncrementedValue);
        }

        [Fact]
        public void IncrementWithTtl_NotExpiredKey()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);

            var ttl = TimeSpan.FromSeconds(3);
            const double addition = 2.0;

            //add a key with some seconds TTL
            _redisService.SetString(keyName, keyValue, ttl);

            _redisService.IncrementString(keyName, ttl, addition);

            var newValue = _redisService.GetString(keyName);

            Assert.Equal(addition + Convert.ToDouble(keyValue), Convert.ToDouble(newValue));

            Task.Delay((int) (ttl.TotalMilliseconds + 200)).GetAwaiter().GetResult();

            var newIncrementedValue = _redisService.GetString(keyName);

            //at this point the incremented ket must be expired again 
            Assert.Null(newIncrementedValue);
        }

        [Fact]
        public async Task IncrementWithDoubleTtlAsync()
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);

            var ttl = TimeSpan.FromSeconds(3.5);
            const double addition = 2.0;

            //at this point the key must be expired 
            await _redisService.IncrementStringAsync(keyName, ttl, addition);

            await Task.Delay((int) (ttl.TotalMilliseconds + 1000));

            var newValue = await _redisService.GetStringAsync(keyName);

            Assert.Null(newValue);
        }

        [Fact]
        public async Task IncrementWithTtlAsync()
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);

            var ttl = TimeSpan.FromSeconds(3);
            const double addition = 2.0;

            //at this point the key must be expired 
            await _redisService.IncrementStringAsync(keyName, ttl, addition);

            await Task.Delay((int) (ttl.TotalMilliseconds + 200));

            var newValue = await _redisService.GetStringAsync(keyName);

            Assert.Null(newValue);
        }

        [Fact]
        public async Task IncrementWithTtlAsync_ExpiredKey()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);

            var ttl = TimeSpan.FromSeconds(3);
            const double addition = 2.0;

            //add a key with some seconds TTL
            await _redisService.SetStringAsync(keyName, keyValue, ttl);

            await Task.Delay((int) (ttl.TotalMilliseconds + 200));

            //at this point the key must be expired 
            await _redisService.IncrementStringAsync(keyName, ttl, addition);

            var newValue = await _redisService.GetStringAsync(keyName);

            Assert.Equal(addition, Convert.ToDouble(newValue));

            await Task.Delay((int) (ttl.TotalMilliseconds + 200));

            var newIncrementedValue = await _redisService.GetStringAsync(keyName);

            //at this point the incremented ket must be expired again 
            Assert.Null(newIncrementedValue);
        }

        [Fact]
        public async Task IncrementWithTtlAsync_ttlWithDoubleType_ExpiredKey()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);

            var ttl = TimeSpan.FromSeconds(3.5);
            const double addition = 2.0;

            //add a key with some seconds TTL
            await _redisService.SetStringAsync(keyName, keyValue, ttl);

            await Task.Delay((int) (ttl.TotalMilliseconds + 1000));

            //at this point the key must be expired 
            await _redisService.IncrementStringAsync(keyName, ttl, addition);

            var newValue = await _redisService.GetStringAsync(keyName);

            Assert.Equal(addition, Convert.ToDouble(newValue));

            await Task.Delay((int) (ttl.TotalMilliseconds + 1000));

            var newIncrementedValue = await _redisService.GetStringAsync(keyName);

            //at this point the incremented ket must be expired again 
            Assert.Null(newIncrementedValue);
        }
    }
}