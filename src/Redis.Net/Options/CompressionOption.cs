namespace Redis.Net.Options
{
    /// <summary>
    /// Options for the compression feature
    /// </summary>
    public class CompressionOption
    {
        /// <summary>
        /// Size in <see cref="byte"/>, greater than this size the compression will be applied
        /// <remarks>for example if the size is 500 * 1024 bytes it means that if the value size of the data
        /// exceeds 500 KB then the library will attempt to gzip the data and also unzip the data when retrieved 
        /// </remarks>
        /// </summary>
        public int TriggerByteSize { get; set; }
        
        /// <summary>
        /// Prefix to detect the base64 strings 
        /// </summary>
        public string Base64Prefix { get; set; } = "####BASE64####";
    }
}