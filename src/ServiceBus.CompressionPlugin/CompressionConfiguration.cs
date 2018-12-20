namespace ServiceBus.CompressionPlugin
{
    using System;

    /// <summary>Runtime configuration for <see cref="CompressionPlugin"/> plugin.</summary>
    public class CompressionConfiguration
    {
        /// <summary>
        /// Compression configuration object to customize <see cref="CompressionPlugin"/>.
        /// </summary>
        /// <param name="compressionMethodName">Compression method name stored as a custom header.</param>
        /// <param name="compressor">Function to compress an array of bytes.</param>
        /// <param name="decompressor">Function to de-compress an array of bytes.</param>
        /// <param name="minimumSize">Minimum size of array for compression to be applied.</param>
        public CompressionConfiguration(string compressionMethodName, Func<byte[], byte[]> compressor, Func<byte[], byte[]> decompressor, int minimumSize)
        {
            Guard.AgainstEmpty(nameof(compressionMethodName), compressionMethodName);
            Guard.AgainstNull(nameof(compressor), compressor);
            Guard.AgainstNull(nameof(decompressor), decompressor);
            Guard.AgainstNegativeOrZero(nameof(minimumSize), minimumSize);

            this.CompressionMethodName = compressionMethodName;
            this.MinimumSize = minimumSize;
            this.Compressor = GetSafeCompressor(compressor);
            this.Decompressor = GetSafeDecompressor(decompressor);
        }

        internal string CompressionMethodName { get; }
        internal int MinimumSize { get; }
        internal Func<byte[], byte[]> Compressor { get; }
        internal Func<byte[], byte[]> Decompressor { get; }

        Func<byte[], byte[]> GetSafeCompressor(Func<byte[], byte[]> userProvidedCompressor)
        {
            return bytes =>
            {
                try
                {
                    return userProvidedCompressor(bytes);
                }
                catch (Exception e)
                {
                    throw new Exception("User provided compression delegate threw an exception.", e);
                }
            };
        }

        Func<byte[], byte[]> GetSafeDecompressor(Func<byte[], byte[]> userProvidedDecompressor)
        {
            return bytes =>
            {
                try
                {
                    return userProvidedDecompressor(bytes);
                }
                catch (Exception e)
                {
                    throw new Exception("User provided decompression delegate threw an exception.", e);
                }
            };
        }
    }
}