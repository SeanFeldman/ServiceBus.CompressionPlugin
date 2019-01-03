namespace Microsoft.Azure.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using global::ServiceBus.CompressionPlugin;

    /// <summary>Runtime configuration for Compression plugin.</summary>
    public class CompressionConfiguration
    {
        Dictionary<string, Func<byte[], byte[]>> mutableDecompressors;

        /// <summary>
        /// Compression configuration object to customize Compression plugin.
        /// <remarks>Additional decompressors can be registered using AddDecompressor(name, function) API.</remarks>
        ///  </summary>
        /// <param name="compressionMethodName">Compression method name stored as a custom header.</param>
        /// <param name="compressor">Function to compress an array of bytes.</param>
        /// <param name="minimumSize">Minimum size of array for compression to be applied.</param>
        /// <param name="decompressor">Single function to decompressor and array of bytes.</param>
        public CompressionConfiguration(string compressionMethodName, Func<byte[], byte[]> compressor, int minimumSize, Func<byte[], byte[]> decompressor)
            : this(compressionMethodName, compressor, minimumSize, new Dictionary<string, Func<byte[], byte[]>> { {compressionMethodName, decompressor} })
        {
        }

        /// <summary>
        /// Compression configuration object to customize Compression plugin.
        /// </summary>
        /// <param name="compressionMethodName">Compression method name stored as a custom header.</param>
        /// <param name="compressor">Function to compress an array of bytes.</param>
        /// <param name="minimumSize">Minimum size of array for compression to be applied.</param>
        /// <param name="decompressors">Dictionary of function to de-compress an array of bytes where keys are compression names.</param>
        public CompressionConfiguration(string compressionMethodName, Func<byte[], byte[]> compressor, int minimumSize, Dictionary<string, Func<byte[], byte[]>> decompressors)
        {
            Guard.AgainstEmpty(nameof(compressionMethodName), compressionMethodName);
            Guard.AgainstNull(nameof(compressor), compressor);
            Guard.AgainstNull(nameof(decompressors), decompressors);
            Guard.GuardAgainstEmptyCollection(nameof(decompressors), decompressors);
            Guard.AgainstNegativeOrZero(nameof(minimumSize), minimumSize);

            CompressionMethodName = compressionMethodName;
            MinimumSize = minimumSize;
            Compressor = GetSafeCompressor(compressor);
            mutableDecompressors = decompressors;
            Decompressors = GetSafeDecompressor(decompressors);
        }

        internal string CompressionMethodName { get; }
        internal int MinimumSize { get; }
        internal Func<byte[], byte[]> Compressor { get; }
        internal Func<string, byte[], byte[]> Decompressors { get; }

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

        Func<string, byte[], byte[]> GetSafeDecompressor(Dictionary<string, Func<byte[], byte[]>> userProvidedDecompressors)
        {
            return (compressionMethodName, bytes) =>
            {
                if (!userProvidedDecompressors.TryGetValue(compressionMethodName, out var decompressor))
                {
                    throw new Exception($"{nameof(CompressionPlugin)} has not been configured to handle messages compressed using '{compressionMethodName}', but a message with this compression has been received. Re-configure plugin to handle '{compressionMethodName}' for decompression.");
                }

                try
                {
                    return decompressor(bytes);
                }
                catch (Exception e)
                {
                    throw new Exception($"User provided decompression delegate for '{compressionMethodName}' compression method threw an exception.", e);
                }
            };
        }

        /// <summary>
        /// Add additional decompression method.
        /// </summary>
        /// <param name="compressionMethodName"></param>
        /// <param name="decompressor"></param>
        public void AddDecompressor(string compressionMethodName, Func<byte[], byte[]> decompressor)
        {
            mutableDecompressors.Add(compressionMethodName, decompressor);
        }
    }
}