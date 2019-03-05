using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class MetadataOracleObjectTests
    {
        public class TestClass
        {

        }

        private void CompareOracleTypeNetMetadata(MetadataOracleTypePropertyDefinition[] expected, MetadataOraclePropertyNetTypeDefinition[] actual)
        {
            Assert.NotEmpty(actual);
            Assert.Collection(actual,
                c =>
                {
                    Assert.NotNull(c);
                    Assert.Equal(c.Name, expected[0].Name, ignoreCase: true);
                    Assert.Equal(c.Order, expected[0].Order);
                },
                c =>
                {
                    Assert.NotNull(c);
                    Assert.Equal(c.Name, expected[1].Name, ignoreCase: true);
                    Assert.Equal(c.Order, expected[1].Order);
                },
                c =>
                {
                    Assert.NotNull(c);
                    Assert.Equal(c.Name, expected[2].Name, ignoreCase: true);
                    Assert.Equal(c.Order, expected[2].Order);
                }
              );
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleObject_Constructor_Object(ServForOracleCache cache, MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch)
        {

            var metadata = new MetadataOracleObject<TestClass>(cache, metadataOracleType, customProperties, fuzzyNameMatch);

            Assert.NotNull(metadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata.Properties);
            CompareOracleTypeNetMetadata(metadataOracleType.Properties.ToArray(), metadata.OracleTypeNetMetadata.Properties.ToArray());
            Assert.Equal(metadataOracleType.UDTInfo, metadata.OracleTypeNetMetadata.UDTInfo);
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleObject_Constructor_Collection(ServForOracleCache cache, MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch)
        {

            var metadata = new MetadataOracleObject<TestClass[]>(cache, metadataOracleType, customProperties, fuzzyNameMatch);

            Assert.NotNull(metadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata.Properties);
            CompareOracleTypeNetMetadata(metadataOracleType.Properties.ToArray(), metadata.OracleTypeNetMetadata.Properties.ToArray());
            Assert.Equal(metadataOracleType.UDTInfo, metadata.OracleTypeNetMetadata.UDTInfo);
        }
    }
}