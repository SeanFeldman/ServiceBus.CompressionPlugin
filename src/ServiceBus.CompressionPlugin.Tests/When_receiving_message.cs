namespace ServiceBus.Tests
{
    using System.Threading.Tasks;
    using CompressionPlugin;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class When_receiving_message
    {
        [Fact]
        public async Task Should_not_attempt_to_decompress_if_no_compression_header_is_found()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var message = new Message(bytes);

            var plugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var sentMessage = await plugin.BeforeMessageSend(message);
            sentMessage.UserProperties[Headers.CompressionMethodName] = "Unknown";

            var receivedMessage = await plugin.AfterMessageReceive(sentMessage);

            Assert.Equal(bytes, receivedMessage.Body);
        }

        [Fact]
        public async Task Should_not_attempt_to_decompress_if_compression_header_is_different_from_configured_method()
        {
            var message = new Message(new byte[] { 1, 2, 3 });
            message.UserProperties[Headers.CompressionMethodName] = "GZip";

            var sendPlugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var sentMessage = await sendPlugin.BeforeMessageSend(message);

            var decompressorExecuted = false;

            var receivePlugin = new CompressionPlugin(
                new CompressionConfiguration(
                    "noop",
                    bytes => bytes,
                    bytes =>
                    {
                        decompressorExecuted = true;
                        return bytes;
                    },
                    1));

            await receivePlugin.AfterMessageReceive(sentMessage);

            Assert.False(decompressorExecuted, "Decompressing function should not have executed, but it did.");
        }

        [Fact]
        public async Task Should_skip_decompression_if_no_compression_header_is_found()
        {
            var message = new Message(new byte[] { 1, 2, 3 });

            var decompressorExecuted = false;

            var receivePlugin = new CompressionPlugin(
                new CompressionConfiguration(
                    "noop",
                    bytes => bytes,
                    bytes =>
                    {
                        decompressorExecuted = true;
                        return bytes;
                    },
                    1));

            await receivePlugin.AfterMessageReceive(message);

            Assert.False(decompressorExecuted, "Decompressing function should not have executed, but it did.");
        }
    }
}
