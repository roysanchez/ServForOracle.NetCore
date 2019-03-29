using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests.Tests
{
    public class MetadataFactoryTests
    {
        [Fact]
        public void CreateBooleanMetadata()
        {
            var metadataFactory = new MetadataFactory();

            MetadataOracleBoolean actual = metadataFactory.CreateBoolean();

            Assert.NotNull(actual);
        }

        [Fact]
        public void CreateCommonMetadata()
        {
            var metadataFactory = new MetadataFactory();

            MetadataOracleCommon actual = metadataFactory.CreateCommon();

            Assert.NotNull(actual);
        }
    }
}
