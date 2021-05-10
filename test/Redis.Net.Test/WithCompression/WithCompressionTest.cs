using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Redis.Net.Contracts;
using Redis.Net.Options;
using Redis.Net.Services;
using Redis.Net.Test.WithCompression.Data;
using StackExchange.Redis;
using Xunit;

namespace Redis.Net.Test.WithCompression
{
    public class WithCompressionTest
    {
        private readonly IRedisService _redisService;
        private const string PersonJsonFileName = "Persons.json";
        private const string PersonJsonGzipBase64FileName = "Persons.json.gzip.bs64";
        private static string KeyName() => Guid.NewGuid().ToString();
        private readonly string _personJsonString;
        private readonly string _personBase64GzippedString;


        public WithCompressionTest()
        {
            //docker run --name some-redis -d -p 6379:6379  redis redis-server --appendonly yes
            var configurationOptions = ConfigurationOptions.Parse("localhost:6379");
            configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
            var options = new RedisCacheOptions()
            {
                ConfigurationOptions = configurationOptions,
                CompressionOption = new CompressionOption()
                {
                    TriggerByteSize = 100 * 1024
                }
            };
            var mock = new Mock<IOptionsMonitor<RedisCacheOptions>>();
            mock.Setup(m => m.Get(nameof(WithCompressionTest))).Returns(options);
            _redisService =
                new RedisService<WithCompressionTest>(mock.Object);
            
            
            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!);
            _personJsonString =  File.ReadAllText(Path.Combine(dataPath, PersonJsonFileName));
            _personBase64GzippedString =  File.ReadAllText(Path.Combine(dataPath, PersonJsonGzipBase64FileName));
        }
        
        [Fact]
        public void SetStringGetString_WithCompression()
        {
            var keyName = KeyName();
            _redisService.Remove(keyName);
            _redisService.SetString(keyName, _personJsonString);
            var result = _redisService.GetString<List<Person>>(keyName);
            _redisService.Remove(keyName);

            var actualObject = JsonConvert.DeserializeObject<List<Person>>(_personJsonString);
            foreach (var person in actualObject)
            {
                Assert.True(result.Exists(z => z.Id.Equals(person.Id)));
            }

            foreach (var person in result)
            {
                Assert.True(actualObject.Exists(z => z.Id.Equals(person.Id)));
            }
        }
        
        
        [Fact]
        public async Task Async_SetStringGetString_WithCompression()
        {
            var keyName = KeyName();
            await _redisService.RemoveAsync(keyName);
            await _redisService.SetStringAsync(keyName, _personJsonString);
            var result = await _redisService.GetStringAsync<List<Person>>(keyName);
            await _redisService.RemoveAsync(keyName);

            var actualObject = JsonConvert.DeserializeObject<List<Person>>(_personJsonString);
            foreach (var person in actualObject)
            {
                Assert.True(result.Exists(z => z.Id.Equals(person.Id)));
            }

            foreach (var person in result)
            {
                Assert.True(actualObject.Exists(z => z.Id.Equals(person.Id)));
            }
        }
    }
}