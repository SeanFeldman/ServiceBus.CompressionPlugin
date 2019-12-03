﻿namespace ServiceBus.CompressionPlugin.Tests
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
                new ApiGeneratorOptions
                {
                    WhitelistedNamespacePrefixes = new[] { "Microsoft.Azure.ServiceBus." },
                    ExcludeAttributes = new[] { "System.Runtime.Versioning.TargetFrameworkAttribute" }
                });

            Approver.Verify(publicApi);
        }
    }
}