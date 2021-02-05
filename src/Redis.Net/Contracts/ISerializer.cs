namespace Redis.Net.Contracts
{
    /// <summary>
    /// A generic serializer contract for the Redis.Net Library conversion from byte to requested types
    /// </summary>
    internal interface ISerializer
    {
        /// <summary>
        /// Convert the input string to requested type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">string as input to be serialized to type <typeparamref name="T"/>.</param>
        /// <typeparam name="T">Generic type conversion support.</typeparam>
        /// <returns>Serialized version of the string to type <typeparamref name="T"/>.</returns>
        T ConvertFromString<T>(string value);

        /// <summary>
        /// Convert the  of type <typeparamref name="T"/> to a string
        /// </summary>
        /// <param name="value">Input value fo type <typeparamref name="T"/></param>
        /// <typeparam name="T">Generic type conversion support</typeparam>
        /// <returns>The string of the object of type <typeparamref name="T"/></returns>
        string ConvertToString<T>(T value);

        /// <summary>
        /// Convert the input byte[] to requested type T 
        /// </summary>
        /// <param name="value">byte array as input to be serialized to type T</param>
        /// <typeparam name="T">Generic type conversion support</typeparam>
        /// <returns>Serialized version of the byte[] to type T</returns>
        T ConvertFromByte<T>(byte[] value);

        /// <summary>
        /// Convert the object of type T to a byte[]
        /// </summary>
        /// <param name="value">Input value fo type T</param>
        /// <typeparam name="T">Generic type conversion support</typeparam>
        /// <returns>The byte[] of the object of type T</returns>
        byte[] ConvertToByte<T>(T value);
    }
}