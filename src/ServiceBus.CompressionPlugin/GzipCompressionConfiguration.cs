namespace ServiceBus.CompressionPlugin
{
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;

    class GzipCompressionConfiguration : CompressionConfiguration
    {
        public GzipCompressionConfiguration() : base("GZip", GzipCompressor, GzipDecompressor, 1500)
        {
        }

        static async Task<byte[]> GzipCompressor(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    await gzipStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                }

                return memoryStream.ToArray();
            }
        }

        static async Task<byte[]> GzipDecompressor(byte[] bytes)
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
                        count = await gzipStream.ReadAsync(buffer, 0, size).ConfigureAwait(false);
                        if (count > 0)
                        {
                            await memory.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                        }
                    }
                    while (count > 0);

                    return memory.ToArray();
                }
            }
        }
    }
}