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

        public override async Task<Message> BeforeMessageSend(Message message)
        {
            if (message.Body == null || message.Body.Length == 0)
            {
                return message;
            }

            // min size should be configurable
            if (message.Body.Length < configuration.MinimumSize)
            {
                return message;
            }

            message.UserProperties[Headers.OriginalBodySize] = message.Body.Length;

            message.Body = await configuration.Compressor(message.Body).ConfigureAwait(false);

            message.UserProperties[Headers.CompressionMethodName] = configuration.CompressionMethodName;
            message.UserProperties[Headers.CompressedBodySize] = message.Body.Length;

            return message;
        }

        public override async Task<Message> AfterMessageReceive(Message message)
        {
            if (message.Body == null || message.Body.Length == 0)
            {
                return message;
            }

            if (!message.UserProperties.TryGetValue(Headers.CompressionMethodName, out var methodName) || (string)methodName != configuration.CompressionMethodName)
            {
                return message;
            }

            message.Body = await configuration.Decompressor(message.Body).ConfigureAwait(false);
            return message;
        }
    }
}
