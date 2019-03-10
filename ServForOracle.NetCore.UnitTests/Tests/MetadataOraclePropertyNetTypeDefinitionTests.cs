using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class MetadataOraclePropertyNetTypeDefinitionTests
    {
        [Fact]
        public void Constructor_NullParameter_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("baseMetadata", () => new MetadataOraclePropertyNetTypeDefinition(null));
        }

        [Theory, CustomAutoData]
        internal void Constructor_OnlySetsNameAndOrder(MetadataOracleTypePropertyDefinition baseMetadata)
        {
            var propertyNet = new MetadataOraclePropertyNetTypeDefinition(baseMetadata);

            Assert.Equal(baseMetadata.Name, propertyNet.Name);
            Assert.Equal(baseMetadata.Order, propertyNet.Order);
            Assert.Null(propertyNet.NETProperty);
            Assert.Null(propertyNet.PropertyMetadata);
        }
    }
}
