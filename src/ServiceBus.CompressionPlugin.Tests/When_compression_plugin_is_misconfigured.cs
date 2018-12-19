namespace ServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using CompressionPlugin;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class When_compression_plugin_is_misconfigured
    {
        [Fact]
        public async Task Should_throw_meaningful_exception_for_erred_compression_func()
        {
            var configuration = new CompressionConfiguration("test", bytes => throw new Exception("compressor threw"), bytes => null, 1);

            var plugin = new CompressionPlugin(configuration);

            var message = new Message(new byte[] { 1, 2, 3 });

            var exception = await Assert.ThrowsAsync<Exception>(() => plugin.BeforeMessageSend(message));

            Assert.StartsWith("User provided compression delegate threw an exception.", exception.Message);
        }

        [Fact]
        public async Task Should_throw_meaningful_exception_for_erred_decompression_func()
        {
            var configuration = new CompressionConfiguration("test", bytes => null, bytes => throw new Exception("decompressor threw"), 1);

            var plugin = new CompressionPlugin(configuration);

            var message = new Message(new byte[] { 1, 2, 3 });
            message.UserProperties[Headers.CompressionMethodName] = configuration.CompressionMethodName;

            var exception = await Assert.ThrowsAsync<Exception>(() => plugin.AfterMessageReceive(message));

            Assert.StartsWith("User provided decompression delegate threw an exception.", exception.Message);
        }
    }
}