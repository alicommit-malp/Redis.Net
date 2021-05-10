using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Redis.Net.Contracts;
using Redis.Net.Options;

namespace Redis.Net.Core
{
    /// <inheritdoc />
    public class Compressor : ICompressor
    {
        private readonly CompressionOption _compressionOption;

        /// <summary>
        /// constructor for receiving the <see cref="CompressionOption"/>
        /// </summary>
        /// <param name="compressionOption"></param>
        public Compressor(CompressionOption compressionOption)
        {
            _compressionOption = compressionOption;
        }

        /// <inheritdoc />
        public async Task<string> CompressAsync(string str, CancellationToken cancellationToken = default)
        {
            if (_compressionOption is null) return str;
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (!IsCompressionRequired(str)) return str;

            var bytes = Encoding.UTF8.GetBytes(str);

            await using var inputStream = new MemoryStream(bytes);
            await using var outputStream = new MemoryStream();
            await using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                CopyTo(inputStream, gZipStream);
            }

            return Convert.ToBase64String(outputStream.ToArray());
        }

        private bool IsCompressionRequired(string str)
        {
            var a = Encoding.UTF8.GetByteCount(str);
            return a >= _compressionOption.TriggerByteSize;
        }

        /// <inheritdoc />
        public string Compress(string str)
        {
            if (_compressionOption is null) return str;
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (!IsCompressionRequired(str)) return str;

            var bytes = Encoding.UTF8.GetBytes(str);

            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                CopyTo(inputStream, gZipStream);
            }

            return Convert.ToBase64String(outputStream.ToArray());
        }

        private static void CopyTo(Stream sourceStream, Stream destinationStream)
        {
            var buffer = new byte[4096];
            int count;
            while ((count = sourceStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                destinationStream.Write(buffer, 0, count);
            }
        }

        private static byte[] FromBase64(string str)
        {
            byte[] result = null;
            try
            {
                result = Convert.FromBase64String(str);
            }
            catch (Exception)
            {
                //ignore
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<string> DeCompressAsync(string str, CancellationToken cancellationToken = default)
        {
            if (_compressionOption is null) return str;
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var fromBase64Bytes = FromBase64(str);
            if (fromBase64Bytes == null) return str;

            await using var msi = new MemoryStream(fromBase64Bytes);
            await using var mso = new MemoryStream();
            await using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                CopyTo(gs, mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }

        /// <inheritdoc />
        public string DeCompress(string str)
        {
            if (_compressionOption is null) return str;
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var fromBase64Bytes = FromBase64(str);
            if (fromBase64Bytes == null) return str;

            using var msi = new MemoryStream(fromBase64Bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                CopyTo(gs, mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }
}