using System.Threading;
using System.Threading.Tasks;

namespace Redis.Net.Contracts
{
    /// <summary>
    /// A contract to be used in compressing and decompressing the data in redis client 
    /// </summary>
    public interface ICompressor
    {
        /// <summary>
        /// Async Compress a string given as input compress it and return base64 version of it
        /// </summary>
        /// <param name="str">any string</param>
        /// <param name="cancellationToken">Optional. The <see cref="CancellationToken"/>
        /// used to propagate notifications that the operation should be canceled.</param>
        /// <returns>Base64 string representation of the compressed data</returns>
        Task<string> CompressAsync(string str,CancellationToken cancellationToken =default);
        
        /// <summary>
        /// Compress a string given as input compress it and return base64 version of it
        /// </summary>
        /// <param name="str">any string</param>
        /// <returns>Base64 string representation of the compressed data</returns>
        string Compress(string str);
        
        /// <summary>
        /// Async Decode base64 and decompress the string 
        /// </summary>
        /// <param name="str">any string</param>
        /// <param name="cancellationToken">Optional. The <see cref="CancellationToken"/>
        /// used to propagate notifications that the operation should be canceled.</param>
        /// <returns>raw decompressed string</returns>
        Task<string> DeCompressAsync(string str,CancellationToken cancellationToken= default);
        
        /// <summary>
        /// Decode base64 and decompress the string 
        /// </summary>
        /// <param name="str">any string</param>
        /// <returns>raw decompressed string</returns>
        string DeCompress(string str);
    }
}