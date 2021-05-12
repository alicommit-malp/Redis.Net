using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Redis.Net.Benchmark.Compression.Data;
using Redis.Net.Contracts;
using Redis.Net.Options;
using Redis.Net.Services;
using StackExchange.Redis;

namespace Redis.Net.Benchmark.Compression
{
    [MemoryDiagnoser]
    public class CompressionBenchmark
    {
        private const string PersonJsonFileName = "Persons.json";

        private const string RedisLocal = "localhost:6379";

        private const string RedisDevRemote = "remote redis connection string";

        private string _redisConnectionString = RedisLocal;

        private const string PersonJsonGzipBase64FileName = "Persons.json.gzip.bs64";
        private readonly string _personJsonString;
        private readonly string _gzippedBase64String;
        private static string KeyName() => Guid.NewGuid().ToString();
        private readonly IRedisService _redisServiceWithCompression;
        private readonly IRedisService _redisServiceWithoutCompression;

        public CompressionBenchmark()
        {
            //docker run --name some-redis -d -p 6379:6379  redis redis-server --appendonly yes
            var configurationOptions = ConfigurationOptions.Parse(_redisConnectionString);
            configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
            var compressionOptions = new CompressionOption()
            {
                TriggerByteSize = 100 * 1024
            };
            var options = new RedisCacheOptions()
            {
                ConfigurationOptions = configurationOptions,
                CompressionOption = compressionOptions
            };
            var mockWithCompression = new Mock<IOptionsMonitor<RedisCacheOptions>>();
            mockWithCompression.Setup(m => m.Get(nameof(CompressionBenchmark))).Returns(options);
            _redisServiceWithCompression =
                new RedisService<CompressionBenchmark>(mockWithCompression.Object);

            var configurationOptions2 = ConfigurationOptions.Parse(_redisConnectionString);
            configurationOptions2.ReconnectRetryPolicy = new ExponentialRetry(1000);
            var options2 = new RedisCacheOptions()
            {
                ConfigurationOptions = configurationOptions2,
            };
            var mockWithoutCompression = new Mock<IOptionsMonitor<RedisCacheOptions>>();
            mockWithoutCompression.Setup(m => m.Get(nameof(CompressionBenchmark))).Returns(options2);
            _redisServiceWithoutCompression =
                new RedisService<CompressionBenchmark>(mockWithoutCompression.Object);

            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!);

            //prepare the person's json data string
            var jsonString = File.ReadAllText(Path.Combine(dataPath, PersonJsonFileName));
            var persons = JsonConvert.DeserializeObject<List<Person>>(jsonString);
            _personJsonString = JsonConvert.SerializeObject(persons);

            //prepare the gzipped Base64 data
            _gzippedBase64String = $"{compressionOptions.Base64Prefix}{File.ReadAllText(Path.Combine(dataPath, PersonJsonGzipBase64FileName))}";
        }

        [Benchmark]
        public async Task WithCompressionBenchmark()
        {
            var keyName = KeyName();
            await _redisServiceWithCompression.RemoveAsync(keyName);
            await _redisServiceWithCompression.SetStringAsync(keyName, _personJsonString);
            var result = await _redisServiceWithCompression.GetStringAsync<List<Person>>(keyName);
            await _redisServiceWithCompression.RemoveAsync(keyName);
        }

        [Benchmark]
        public async Task WithoutCompressionBenchmark()
        {
            var keyName = KeyName();
            await _redisServiceWithoutCompression.RemoveAsync(keyName);
            await _redisServiceWithoutCompression.SetStringAsync(keyName, _personJsonString);
            var result = await _redisServiceWithoutCompression.GetStringAsync<List<Person>>(keyName);
            await _redisServiceWithoutCompression.RemoveAsync(keyName);
        }
    }
}