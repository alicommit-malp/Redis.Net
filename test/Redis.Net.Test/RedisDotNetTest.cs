using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Options;
using Moq;
using Redis.Net.Contracts;
using Redis.Net.Options;
using Redis.Net.Services;
using StackExchange.Redis;
using Xunit;

namespace Redis.Net.Test
{
    public class RedisDotNetTest
    {
        private readonly IRedisService _redisService;
        private static string KeyName() => Guid.NewGuid().ToString();

        public RedisDotNetTest()
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

        private class MyClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public async Task SetStringGetStringRedisAsyncTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, keyValue);
            var result = await _redisService.GetStringAsync<string>(keyName);
            await _redisService.RemoveAsync(keyName);

            Assert.Equal(keyValue, result);
        }

        [Fact]
        public void SetStringGetStringRedisTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, keyValue);
            var result = _redisService.GetString<string>(keyName);
            _redisService.Remove(keyName);

            Assert.Equal(keyValue, result);
        }

        [Fact]
        public async Task SetStringGetStringRedisAsyncWithExpiryTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, keyValue, TimeSpan.FromSeconds(2));
            var result = await _redisService.GetStringAsync<string>(keyName);

            Assert.Equal(keyValue, result);

            await Task.Delay(3000);
            var resultAfterExpiry = await _redisService.GetStringAsync<string>(keyName);
            Assert.Null(resultAfterExpiry);

            await _redisService.RemoveAsync(keyName);
        }

        [Fact]
        public void SetStringGetStringRedisWithExpiryTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, keyValue, TimeSpan.FromSeconds(2));
            var result = _redisService.GetString<string>(keyName);

            Assert.Equal(keyValue, result);

            Task.Delay(3000).GetAwaiter().GetResult();
            var resultAfterExpiry = _redisService.GetString<string>(keyName);
            Assert.Null(resultAfterExpiry);

            _redisService.Remove(keyName);
        }


        [Fact]
        public async Task GetSetRedisAsyncTest()
        {
            var testClass = new MyClass()
            {
                Name = "Ali Alp",
                Age = 38
            };

            await _redisService.SetAsync(testClass.Name, testClass, new CacheOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });

            var result = await _redisService.GetAsync<MyClass>(testClass.Name);
            await _redisService.RemoveAsync(testClass.Name);

            Assert.Equal(testClass.Name, result.Name);
            Assert.Equal(testClass.Age, result.Age);
        }

        [Fact]
        public void GetSetNoOptionsRedisTest()
        {
            var testClass = new MyClass()
            {
                Name = "Ali Alp",
                Age = 38
            };
            _redisService.Set(testClass.Name, testClass);

            var result = _redisService.Get<MyClass>(testClass.Name);

            Assert.Equal(testClass.Name, result.Name);
            Assert.Equal(testClass.Age, result.Age);

            Task.Delay(5100).GetAwaiter().GetResult();


            var result2 = _redisService.Get<MyClass>(testClass.Name);
            Assert.Equal(testClass.Name, result2.Name);
            Assert.Equal(testClass.Age, result2.Age);

            _redisService.RemoveAsync(testClass.Name);
        }

        [Fact]
        public void GetSetRedisTest()
        {
            var testClass = new MyClass()
            {
                Name = "Ali Alp",
                Age = 38
            };
            _redisService.Set(testClass.Name, testClass, new CacheOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(4)
            });

            var result = _redisService.Get<MyClass>(testClass.Name);

            Assert.Equal(testClass.Name, result.Name);
            Assert.Equal(testClass.Age, result.Age);

            Task.Delay(5100).GetAwaiter().GetResult();


            var result2 = _redisService.Get<MyClass>(testClass.Name);
            Assert.Null(result2);

            _redisService.RemoveAsync(testClass.Name);
        }


        [Fact]
        public void ExpireKeyTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, keyValue);

            //check if the key exist 
            var result = _redisService.GetString(keyName);
            Assert.Equal(keyValue, result);

            //expire the key
            _redisService.ExpireKey(keyName, TimeSpan.FromSeconds(2));
            Task.Delay(2100).GetAwaiter().GetResult();

            //check if the key exist 
            var result2 = _redisService.GetString(keyName);
            Assert.Null(result2);
        }


        [Fact]
        public async Task ExpireKeyAsyncTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, keyValue);

            //check if the key exist 
            var result = await _redisService.GetStringAsync(keyName);
            Assert.Equal(keyValue, result);

            //expire the key
            await _redisService.ExpireKeyAsync(keyName, TimeSpan.FromSeconds(2));
            await Task.Delay(2100);

            //check if the key exist 
            var result2 = await _redisService.GetStringAsync(keyName);
            Assert.Null(result2);
        }

        [Fact]
        public async Task KeyExistAsyncTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, keyValue);

            //check if the key exist 
            var result = await _redisService.IsKeyExistAsync(keyName);
            Assert.True(result);

            //expire the key
            await _redisService.RemoveAsync(keyName);

            //check if the key exist 
            var result2 = await _redisService.IsKeyExistAsync(keyName);
            Assert.False(result2);
        }


        [Fact]
        public void KeyExistTest()
        {
            var keyName = KeyName();
            const string keyValue = "1.0";
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, keyValue);

            //check if the key exist 
            var result = _redisService.IsKeyExist(keyName);
            Assert.True(result);

            _redisService.Remove(keyName);

            //check if the key exist 
            var result2 = _redisService.IsKeyExist(keyName);
            Assert.False(result2);
        }
        
        
        [Fact]
        public async Task GetKeysAsync_Success()
        {
            var keyName1 = KeyName() + "ali" + KeyName();
            var keyName2 = KeyName() + "ali" + KeyName();
            var keyName3 = KeyName() + "ali" + KeyName();
            var keyName4 = KeyName() + "li" + KeyName();
            var keyName5 = KeyName() + "al" + KeyName();

            await _redisService.SetStringAsync(keyName1, "");
            await _redisService.SetStringAsync(keyName2, "");
            await _redisService.SetStringAsync(keyName3, "");
            await _redisService.SetStringAsync(keyName4, "");
            await _redisService.SetStringAsync(keyName5, "");

            var result = await _redisService.GetKeysAsync("*ali*");

            Assert.NotNull(result);
            var enumerable = result.ToList();
            Assert.NotNull(enumerable.FirstOrDefault(z => z.Contains(keyName1)));
            Assert.NotNull(enumerable.FirstOrDefault(z => z.Contains(keyName2)));
            Assert.NotNull(enumerable.FirstOrDefault(z => z.Contains(keyName3)));

            await _redisService.RemoveAsync(keyName1);
            await _redisService.RemoveAsync(keyName2);
            await _redisService.RemoveAsync(keyName3);
            await _redisService.RemoveAsync(keyName4);
            await _redisService.RemoveAsync(keyName5);
        }

        [Fact]
        public void GetKeys_Success()
        {
            var randomName = new Faker().Name.FirstName();
            var anotherRandomName = new Faker().Name.FirstName();

            var keyName1 = KeyName() + randomName + KeyName();
            var keyName2 = KeyName() + randomName + KeyName();
            var keyName3 = KeyName() + randomName + KeyName();
            var keyName4 = KeyName() + anotherRandomName + KeyName();
            var keyName5 = KeyName() + anotherRandomName + KeyName();

            _redisService.SetString(keyName1, "");
            _redisService.SetString(keyName2, "");
            _redisService.SetString(keyName3, "");
            _redisService.SetString(keyName4, "");
            _redisService.SetString(keyName5, "");

            var result = _redisService.GetKeys("*" + randomName + "*");

            Assert.NotNull(result);
            var enumerable = result.ToList();
            Assert.NotNull(enumerable.FirstOrDefault(z => z.Contains(keyName1)));
            Assert.NotNull(enumerable.FirstOrDefault(z => z.Contains(keyName2)));
            Assert.NotNull(enumerable.FirstOrDefault(z => z.Contains(keyName3)));

            _redisService.Remove(keyName1);
            _redisService.Remove(keyName2);
            _redisService.Remove(keyName3);
            _redisService.Remove(keyName4);
            _redisService.Remove(keyName5);
        }

        [Fact]
        public async Task RemoveKeysAsync_Success()
        {
            var randomName = new Faker().Name.FirstName();

            var keyName1 = KeyName() + randomName + KeyName();
            var keyName2 = KeyName() + randomName + KeyName();
            var keyName3 = KeyName() + randomName + KeyName();

            await _redisService.SetStringAsync(keyName1, "");
            await _redisService.SetStringAsync(keyName2, "");
            await _redisService.SetStringAsync(keyName3, "");

            await _redisService.RemoveKeysAsync("*" + randomName + "*");
            var result = await _redisService.GetKeysAsync("*" + randomName + "*");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void RemoveKeys_Success()
        {
            var randomName = new Faker().Name.FirstName();

            var keyName1 = KeyName() + randomName + KeyName();
            var keyName2 = KeyName() + randomName + KeyName();
            var keyName3 = KeyName() + randomName + KeyName();

            _redisService.SetString(keyName1, "");
            _redisService.SetString(keyName2, "");
            _redisService.SetString(keyName3, "");

            _redisService.RemoveKeys("*" + randomName + "*");
            var result = _redisService.GetKeys("*" + randomName + "*");

            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public async Task GetKeysCountAsync_Success()
        {
            var randomName = new Faker().Name.FirstName();
            var anotherRandomName = new Faker().Name.FirstName();

            var keyName1 = KeyName() + randomName + KeyName();
            var keyName2 = KeyName() + randomName + KeyName();
            var keyName3 = KeyName() + randomName + KeyName();
            var keyName4 = KeyName() + anotherRandomName + KeyName();
            var keyName5 = KeyName() + anotherRandomName + KeyName();

            await _redisService.SetStringAsync(keyName1, "");
            await _redisService.SetStringAsync(keyName2, "");
            await _redisService.SetStringAsync(keyName3, "");
            await _redisService.SetStringAsync(keyName4, "");
            await _redisService.SetStringAsync(keyName5, "");

            var result = await _redisService.GetKeysCountAsync("*"+randomName+"*");

            Assert.Equal(3, result);

            await _redisService.RemoveAsync(keyName1);
            await _redisService.RemoveAsync(keyName2);
            await _redisService.RemoveAsync(keyName3);
            await _redisService.RemoveAsync(keyName4);
            await _redisService.RemoveAsync(keyName5);
        }

        [Fact]
        public void GetKeysCount_Success()
        {
            var randomName = new Faker().Name.FirstName();
            var anotherRandomName = new Faker().Name.FirstName();

            var keyName1 = KeyName() + randomName + KeyName();
            var keyName2 = KeyName() + randomName + KeyName();
            var keyName3 = KeyName() + randomName + KeyName();
            var keyName4 = KeyName() + anotherRandomName + KeyName();
            var keyName5 = KeyName() + anotherRandomName + KeyName();

            _redisService.SetString(keyName1, "");
            _redisService.SetString(keyName2, "");
            _redisService.SetString(keyName3, "");
            _redisService.SetString(keyName4, "");
            _redisService.SetString(keyName5, "");

            var result = _redisService.GetKeysCount("*"+randomName+"*");

            Assert.Equal(3, result);

            _redisService.Remove(keyName1);
            _redisService.Remove(keyName2);
            _redisService.Remove(keyName3);
            _redisService.Remove(keyName4);
            _redisService.Remove(keyName5);
        }
    }
}