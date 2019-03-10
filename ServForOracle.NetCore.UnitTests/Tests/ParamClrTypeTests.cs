using Moq;
using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ParamClrTypeTests
    {
        [Theory, CustomAutoData]
        public void Constructor_TwoParameters(string value, ParameterDirection direction)
        {
            var param = new ParamClrType<string>(value, direction);

            Assert.Equal(value, param.Value);
            Assert.Equal(direction, param.Direction);
            Assert.NotNull(param.Metadata);
        }

        [Theory, CustomAutoData]
        internal void Constructor_ThreeParameters(string value, ParameterDirection direction, MetadataOracleCommon metadata)
        {
            var param = new ParamClrType<string>(value, direction, metadata);

            Assert.Equal(value, param.Value);
            Assert.Equal(direction, param.Direction);
            Assert.Equal(metadata, param.Metadata);
        }

        [Theory, CustomAutoData]
        internal void SetOutputValue_CallsMetadata(string value, ParameterDirection direction, Mock<MetadataOracleCommon> metadata, string expectedValue)
        {
            metadata.Setup(m => m.ConvertOracleParameterToBaseType(typeof(string), null))
                .Returns(expectedValue);

            var param = new ParamClrType<string>(value, direction, metadata.Object);

            param.SetOutputValue(null);

            Assert.Equal(expectedValue, param.Value);
        }

        [Theory, CustomAutoData]
        internal async Task SetOutputValueAsync_CallsMetadata(string value, ParameterDirection direction, Mock<MetadataOracleCommon> metadata, string expectedValue)
        {
            metadata.Setup(m => m.ConvertOracleParameterToBaseType(typeof(string), null))
                .Returns(expectedValue);

            var param = new ParamClrType<string>(value, direction, metadata.Object);

            await param.SetOutputValueAsync(null);

            Assert.Equal(expectedValue, param.Value);
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameter_CallsMetadata(string value, ParameterDirection direction, Mock<MetadataOracleCommon> metadata, string name, OracleParameter oracleParameter)
        {
            metadata.Setup(m => m.GetOracleParameter(typeof(string), direction, name, value))
                .Returns(oracleParameter);

            var param = new ParamClrType<string>(value, direction, metadata.Object);

            var actual = param.GetOracleParameter(name);

            Assert.Equal(oracleParameter, actual);
        }
    }
}
