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
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(CompressionPlugin).Assembly,
                whitelistedNamespacePrefixes: new[] { "Microsoft.Azure.ServiceBus." },
                excludeAttributes: new[] { "System.Runtime.Versioning.TargetFrameworkAttribute" });

            Approver.Verify(publicApi);
        }
    }
}