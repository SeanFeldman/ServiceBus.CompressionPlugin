namespace ServiceBus.CompressionPlugin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using ServiceBus.CompressionPlugin;
    using Xunit;

    public class When_compression_plugin_is_misconfigured
    {
        [Fact]
        public async Task Should_throw_meaningful_exception_for_erred_compression_func()
        {
            var configuration = new CompressionConfiguration("test", bytes => throw new Exception("compressor threw"), 1,
                new Dictionary<string, Func<byte[], byte[]>> { { "test", bytes => null } });

            var plugin = new CompressionPlugin(configuration);

            var message = new Message(new byte[] { 1, 2, 3 });

            var exception = await Assert.ThrowsAsync<Exception>(() => plugin.BeforeMessageSend(message));

            Assert.StartsWith("User provided compression delegate threw an exception.", exception.Message);
        }

        [Fact]
        public async Task Should_throw_meaningful_exception_for_erred_decompression_func()
        {
            var configuration = new CompressionConfiguration("test", bytes => null, 1,
                new Dictionary<string, Func<byte[], byte[]>> { { "test", bytes => throw new Exception("decompressor threw") } });

            var plugin = new CompressionPlugin(configuration);

            var message = new Message(new byte[] { 1, 2, 3 });
            message.UserProperties[Headers.CompressionMethodName] = configuration.CompressionMethodName;

            var exception = await Assert.ThrowsAsync<Exception>(() => plugin.AfterMessageReceive(message));

            Assert.StartsWith($"User provided decompression delegate for '{configuration.CompressionMethodName}' compression method threw an exception.", exception.Message);
        }

        [Fact]
        public async Task Should_throw_when_cannot_decompress_due_to_missing_decompressor_func()
        {
            var configuration = new CompressionConfiguration("test", bytes => null, 1,
                new Dictionary<string, Func<byte[], byte[]>> { { "test", bytes => throw new Exception("decompressor threw") } });

            var plugin = new CompressionPlugin(configuration);

            var message = new Message(new byte[] { 1, 2, 3 });
            message.UserProperties[Headers.CompressionMethodName] = "unknown-compression";

            var exception = await Assert.ThrowsAsync<Exception>(() => plugin.AfterMessageReceive(message));

            Assert.StartsWith($"{nameof(CompressionPlugin)} has not been configured to handle messages compressed using", exception.Message);
        }
    }
}