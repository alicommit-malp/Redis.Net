using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.Net.Contracts;
using Redis.Net.Core;
using Redis.Net.Options;
using Redis.Net.Test.WithCompression.Data;
using Xunit;

namespace Redis.Net.Test.WithCompression
{
    public class CompressorUnitTest
    {
        private const string PersonJsonFileName = "Persons.json";
        private const string PersonJsonGzipBase64FileName = "Persons.json.gzip.bs64";
        private readonly ICompressor _compressor;
        private readonly string _personJsonString;
        private readonly string _personBase64GzippedString;

        public CompressorUnitTest()
        {
            var compressionOptions = new CompressionOption()
            {
                TriggerByteSize = 100 * 1024
            };
            _compressor = new Compressor(compressionOptions);

            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!);
            _personJsonString = File.ReadAllText(Path.Combine(dataPath, PersonJsonFileName));
            _personBase64GzippedString =  $"{compressionOptions.Base64Prefix}{File.ReadAllText(Path.Combine(dataPath, PersonJsonGzipBase64FileName))}";
        }

        [Fact]
        public async Task Async_CompressWithGzip_returnGzippedVersion()
        {
            var persons = JsonConvert.DeserializeObject<List<Person>>(_personJsonString);
            var personsJsonString = JsonConvert.SerializeObject(persons);
            var gzippedString = await _compressor.CompressAsync(personsJsonString);
            Assert.Equal(_personBase64GzippedString, gzippedString);
        }

        [Fact]
        public async Task Async_DeCompressWithGzip_returnJsonStringVersion()
        {
            var unzippedString = await _compressor.DeCompressAsync(_personBase64GzippedString);
            var decompressedObject = JsonConvert.DeserializeObject<List<Person>>(unzippedString);
            var actualObject = JsonConvert.DeserializeObject<List<Person>>(_personJsonString);

            foreach (var person in actualObject)
            {
                Assert.True(decompressedObject.Exists(z => z.Id.Equals(person.Id)));
            }

            foreach (var person in decompressedObject)
            {
                Assert.True(actualObject.Exists(z => z.Id.Equals(person.Id)));
            }
        }

        [Fact]
        public void CompressWithGzip_returnGzippedVersion()
        {
            var persons = JsonConvert.DeserializeObject<List<Person>>(_personJsonString);
            var personsJsonString = JsonConvert.SerializeObject(persons);
            var gzippedString = _compressor.Compress(personsJsonString);
            Assert.Equal(_personBase64GzippedString, gzippedString);
        }

        [Fact]
        public void DeCompressWithGzip_returnJsonStringVersion()
        {
            var unzippedString = _compressor.DeCompress(_personBase64GzippedString);
            var decompressedObject = JsonConvert.DeserializeObject<List<Person>>(unzippedString);
            var actualObject = JsonConvert.DeserializeObject<List<Person>>(_personJsonString);

            foreach (var person in actualObject)
            {
                Assert.True(decompressedObject.Exists(z => z.Id.Equals(person.Id)));
            }

            foreach (var person in decompressedObject)
            {
                Assert.True(actualObject.Exists(z => z.Id.Equals(person.Id)));
            }
        }
    }
}