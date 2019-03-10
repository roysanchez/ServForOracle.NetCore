using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class OracleUdtAttributeTests
    {
        [Theory, CustomAutoData]
        public void Constructor_OneParameter(string schema, string name)
        {
            var attribute = new OracleUdtAttribute($"{schema}.{name}");

            Assert.NotNull(attribute.UDTInfo);
        }

        [Theory, CustomAutoData]
        public void Constructor_TwoParameter(string schema, string name)
        {
            var attribute = new OracleUdtAttribute(schema, name);

            Assert.NotNull(attribute.UDTInfo);
        }

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameter(string schema, string name, string collection)
        {
            var attribute = new OracleUdtAttribute(schema, name, collection);

            Assert.NotNull(attribute.UDTInfo);
        }

        [Theory, CustomAutoData]
        public void Constructor_FourParameter(string schema, string name, string collectionSchema, string collectionName)
        {
            var attribute = new OracleUdtAttribute(schema, name, collectionSchema, collectionName);

            Assert.NotNull(attribute.UDTInfo);
        }
    }
}
