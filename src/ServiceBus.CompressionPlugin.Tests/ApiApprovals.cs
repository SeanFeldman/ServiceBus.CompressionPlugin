namespace ServiceBus.CompressionPlugin.Tests
{
    using PublicApiGenerator;
    using ServiceBus.CompressionPlugin;
    using Xunit;

    public class ApiApprovals
    {
        [Fact]
        public void CompressionPlugin()
        {
            var publicApi = typeof(CompressionPlugin).Assembly.GeneratePublicApi(
                new ApiGeneratorOptions
                {
                    WhitelistedNamespacePrefixes = new[] {"Microsoft.Azure.ServiceBus."},
                    ExcludeAttributes = new[]
                    {
                        "System.Runtime.Versioning.TargetFrameworkAttribute",
                        "System.Reflection.AssemblyMetadataAttribute"
                    }
                });

            Approver.Verify(publicApi);
        }
    }
}