namespace ServiceBus.CompressionPlugin
{
    using System.IO;
    using System.IO.Compression;
    using Microsoft.Azure.ServiceBus;

    class GzipCompressionConfiguration : CompressionConfiguration
    {
        public GzipCompressionConfiguration() : base("GZip", GzipCompressor, GzipDecompressor, 1500)
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
            using (var gzipStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
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