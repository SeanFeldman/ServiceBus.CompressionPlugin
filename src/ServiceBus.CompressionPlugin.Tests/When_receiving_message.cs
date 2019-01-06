namespace ServiceBus.CompressionPlugin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using ServiceBus.CompressionPlugin;
    using Xunit;

    public class When_receiving_message
    {
        [Fact]
        public async Task Should_skip_decompression_if_no_compression_header_is_found()
        {
            var body = new byte[] { 1, 2, 3 };
            var message = new Message(body);

            var decompressorExecuted = false;

            var receivePlugin = new CompressionPlugin(
                new CompressionConfiguration("noop", bytes => bytes, 1,
                    new Dictionary<string, Func<byte[], byte[]>>
                    {
                        {
                            "compression-x", bytes =>
                            {
                                decompressorExecuted = true;
                                return bytes;
                            }
                        }
                    }));

            var receivedMessage = await receivePlugin.AfterMessageReceive(message);

            Assert.Equal(body, receivedMessage.Body);

            Assert.False(decompressorExecuted, "Decompressing function should not have executed, but it did.");
        }

        [Fact]
        public async Task Should_throw_for_not_configured_compression_method()
        {
            var bytes = new byte[] {1, 2, 3};
            var message = new Message(bytes);

            var plugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var sentMessage = await plugin.BeforeMessageSend(message);
            sentMessage.UserProperties[Headers.CompressionMethodName] = "Unknown";

            await Assert.ThrowsAsync<Exception>(() => plugin.AfterMessageReceive(sentMessage));
        }

        [Fact]
        public async Task Should_be_able_to_decompress_messages_sent_with_different_compression_other_than_default()
        {
            var deflateCompressionConfiguration = new DeflateCompressionConfiguration();

            var gzipCompressionConfiguration = new GzipCompressionConfiguration(1);
            gzipCompressionConfiguration.AddDecompressor(deflateCompressionConfiguration.CompressionMethodName, DeflateDecompressor);

            var bytes = Enumerable.Range(1, 1024).Select(x => (byte)x).ToArray();
            var message = new Message(bytes);
            message.UserProperties[Headers.CompressionMethodName] = deflateCompressionConfiguration.CompressionMethodName;

            var sendPlugin = new CompressionPlugin(deflateCompressionConfiguration);
            var sentMessage = await sendPlugin.BeforeMessageSend(message);

            var receivePlugin = new CompressionPlugin(gzipCompressionConfiguration);
            var receivedMessage = await receivePlugin.AfterMessageReceive(sentMessage);

            Assert.Equal(bytes, receivedMessage.Body);
        }

        class DeflateCompressionConfiguration : CompressionConfiguration
        {
            public DeflateCompressionConfiguration() : base("Deflate", DeflateCompressor, 1, DeflateDecompressor) { }
        }

        static byte[] DeflateCompressor(byte[] bytes)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                compressor.Write(bytes, 0, bytes.Length);
                compressor.Flush();
                compressor.Close();
                return compressStream.GetBuffer();
            }
        }

        static byte[] DeflateDecompressor(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            using (var decompressStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = decompressStream.Read(buffer, 0, size);
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
