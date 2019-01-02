namespace ServiceBus.CompressionPlugin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Xunit;

    public class When_configuring_plugin
    {
        [Fact]
        public async Task Should_be_able_to_override_minimu_size_for_compression()
        {
            var client = new FakeClient("fake-client", string.Empty, RetryPolicy.NoRetry);

            var plugin = CompressionPluginExtensions.RegisterCompressionPlugin(client, 2);

            var message = new Message(new byte[] { 1, 2, 3 });

            await plugin.BeforeMessageSend(message);

            Assert.Equal(2, client.RegisteredPluginMinimumCompressionSize);
        }

        class FakeClient : ClientEntity
        {
            public int RegisteredPluginMinimumCompressionSize;

            public FakeClient(string clientTypeName, string postfix, RetryPolicy retryPolicy) : base(clientTypeName, postfix, retryPolicy)
            {
            }

            public override void RegisterPlugin(ServiceBusPlugin serviceBusPlugin)
            {
                var plugin = serviceBusPlugin as CompressionPlugin;
                RegisteredPluginMinimumCompressionSize = plugin.configuration.MinimumSize;
            }

            public override void UnregisterPlugin(string serviceBusPluginName)
            {
                throw new NotImplementedException();
            }

            public override string Path { get; }
            public override TimeSpan OperationTimeout { get; set; }

            protected override Task OnClosingAsync()
            {
                throw new NotImplementedException();
            }

            public override ServiceBusConnection ServiceBusConnection { get; }
            public override IList<ServiceBusPlugin> RegisteredPlugins { get; }
        }
    }
}