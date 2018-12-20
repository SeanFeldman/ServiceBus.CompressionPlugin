namespace ServiceBus.CompressionPlugin
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    class CompressionPlugin : ServiceBusPlugin
    {
        CompressionConfiguration configuration;

        public CompressionPlugin(CompressionConfiguration configuration)
        {
            Guard.AgainstNull(nameof(configuration), configuration);

            this.configuration = configuration;
        }

        public override string Name => nameof(CompressionPlugin);

        public override bool ShouldContinueOnException { get; } = false;

        public override Task<Message> BeforeMessageSend(Message message)
        {
            if (message.Body == null || message.Body.Length == 0)
            {
                return Task.FromResult(message);
            }

            // min size should be configurable
            if (message.Body.Length < configuration.MinimumSize)
            {
                return Task.FromResult(message);
            }

            message.UserProperties[Headers.OriginalBodySize] = message.Body.Length;

            message.Body = configuration.Compressor(message.Body);

            message.UserProperties[Headers.CompressionMethodName] = configuration.CompressionMethodName;
            message.UserProperties[Headers.CompressedBodySize] = message.Body.Length;

            return Task.FromResult(message);
        }

        public override Task<Message> AfterMessageReceive(Message message)
        {
            if (message.Body == null || message.Body.Length == 0)
            {
                return Task.FromResult(message);
            }

            if (!message.UserProperties.TryGetValue(Headers.CompressionMethodName, out var methodName) || (string)methodName != configuration.CompressionMethodName)
            {
                return Task.FromResult(message);
            }

            message.Body = configuration.Decompressor(message.Body);
            return Task.FromResult(message);
        }
    }
}
