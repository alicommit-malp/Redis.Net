using System.Text;
using Newtonsoft.Json;
using Redis.Net.Contracts;

namespace Redis.Net.Core
{
    /// <inheritdoc />
    internal class Serializer : ISerializer
    {
        /// <inheritdoc />
        public T ConvertFromString<T>(string value)
        {
            return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        /// <inheritdoc />
        public string ConvertToString<T>(T value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <inheritdoc />
        public T ConvertFromByte<T>(byte[] value)
        {
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(value));
        }

        /// <inheritdoc />
        public byte[] ConvertToByte<T>(T value)
        {
            return Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(value));
        }
    }
}