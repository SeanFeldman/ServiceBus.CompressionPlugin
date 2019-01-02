namespace Microsoft.Azure.ServiceBus
{
    using Core;
    using global::ServiceBus.CompressionPlugin;

    /// <summary>
    /// Compression plugin registration options.
    /// </summary>
    public static class CompressionPluginExtensions
    {
        /// <summary>
        /// Register compression plugin.
        /// <remarks>GZip compression will be used.</remarks>
        /// <param name="client"></param>
        /// <param name="minimumSizeToApplyCompression">Minimum size of message body for compression to be applied.</param>
        /// </summary>
        public static ServiceBusPlugin RegisterCompressionPlugin(this ClientEntity client, int minimumSizeToApplyCompression = GzipCompressionConfiguration.MinimumCompressionSize)
        {
            return client.RegisterCompressionPlugin(new GzipCompressionConfiguration(minimumSizeToApplyCompression));
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