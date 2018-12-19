namespace ServiceBus.CompressionPlugin
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    /// <summary>
    /// <see cref="CompressionPlugin"/> registration options.
    /// </summary>
    public static class CompressionPluginExtensions
    {
        /// <summary>
        /// Register compression plugin.
        /// <remarks>GZip compression will be used.</remarks>
        /// </summary>
        public static ServiceBusPlugin RegisterCompressionPlugin(this ClientEntity client)
        {
            return client.RegisterCompressionPlugin(new GzipCompressionConfiguration());
        }

        /// <summary>
        /// Register compression plugin with customizations.
        /// </summary>
        /// <param name="compressionConfiguration">Compression to be used.</param>
        /// <param name="client"></param>
        public static ServiceBusPlugin RegisterCompressionPlugin(this ClientEntity client, CompressionConfiguration compressionConfiguration)
        {
            ServiceBusPlugin plugin = new CompressionPlugin(compressionConfiguration);

            client.RegisterPlugin(plugin);

            return plugin;
        }
    }
}