namespace ServiceBus.CompressionPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using Microsoft.Azure.ServiceBus;

    class GzipCompressionConfiguration : CompressionConfiguration
    {
        public const int MinimumCompressionSize = 1500;

        public GzipCompressionConfiguration(int minimumSizeToApplyCompression = MinimumCompressionSize)
            : base("GZip", GzipCompressor, minimumSizeToApplyCompression, new Dictionary<string, Func<byte[], byte[]>> { {"GZip", GzipDecompressor} })
        {
        }

        static byte[] GzipCompressor(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                }

                return memoryStream.ToArray();
            }
        }

        static byte[] GzipDecompressor(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = gzipStream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);

                    return memory.ToArray();
                }
            }
        }
    }
}