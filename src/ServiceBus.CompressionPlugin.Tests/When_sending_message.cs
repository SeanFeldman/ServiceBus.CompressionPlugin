namespace ServiceBus.CompressionPlugin.Tests
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using ServiceBus.CompressionPlugin;
    using Xunit;

    public class When_sending_message
    {
        [Fact]
        public async Task Should_not_compress_null_body()
        {
            var message = new Message(null);

            var plugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var result = await plugin.BeforeMessageSend(message);

            Assert.Null(result.Body);
            Assert.False(message.UserProperties.ContainsKey(Headers.CompressionMethodName), "Header should not be found for a message with null body.");
        }

        [Fact]
        public async Task Should_not_compress_empty_body()
        {
            var message = new Message(new byte[] {});

            var plugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var result = await plugin.BeforeMessageSend(message);

            Assert.Equal(Array.Empty<byte>(), result.Body);
            Assert.False(message.UserProperties.ContainsKey(Headers.CompressionMethodName), "Header should not be found for a message with empty body.");
        }

        [Fact]
        public async Task Should_not_compress_body_smaller_than_configured_size()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var message = new Message(bytes);

            var plugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var result = await plugin.BeforeMessageSend(message);

            Assert.Equal(bytes, result.Body);
            Assert.False(message.UserProperties.ContainsKey(Headers.CompressionMethodName), "Header should not be found for a message with empty body.");
        }


        [Fact]
        public async Task Should_compress_normal_body()
        {
            var payload = new string('A', 1500);
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);

            var plugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var result = await plugin.BeforeMessageSend(message);

            var bodyAsBase64String = Convert.ToBase64String(result.Body);

#if NETFRAMEWORK
            Assert.Equal("H4sIAAAAAAAEAHN0HAWjYBSMglEw3AAATW9s69wFAAA=", bodyAsBase64String);
            Assert.Equal(32, message.UserProperties[Headers.CompressedBodySize]);
#else
            Assert.Equal("H4sIAAAAAAAAC3N0HAWjITAaAqMhMBoCjsMMAABNb2zr3AUAAA==", bodyAsBase64String);
            Assert.Equal(37, message.UserProperties[Headers.CompressedBodySize]);
#endif
            Assert.Equal("GZip", message.UserProperties[Headers.CompressionMethodName]);
            Assert.Equal(payload.Length, message.UserProperties[Headers.OriginalBodySize]);
        }

        [Fact]
        public async Task Should_receive_it()
        {
            var payload = new string('A', 1500);
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);

            var plugin = new CompressionPlugin(new GzipCompressionConfiguration());
            var result = await plugin.BeforeMessageSend(message);

            var receivedMessage = await plugin.AfterMessageReceive(result);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
            Assert.Equal("GZip", message.UserProperties[Headers.CompressionMethodName]);
        }
    }
}