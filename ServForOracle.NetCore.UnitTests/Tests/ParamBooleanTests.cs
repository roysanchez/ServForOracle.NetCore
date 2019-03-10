using Moq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
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
    public class ParamBooleanTests
    {
        [Theory, CustomAutoData]
        public void Constructor_OneParameter(bool? value, ParameterDirection direction)
        {
            var param = new ParamBoolean(value, direction);

            Assert.Equal(value, param.Value);
            Assert.Equal(direction, param.Direction);
            Assert.NotNull(param.Metadata);
        }

        [Theory, CustomAutoData]
        internal void Constructor_TwoParameter(bool? value, ParameterDirection direction, MetadataOracleBoolean metadata)
        {
            var param = new ParamBoolean(value, direction, metadata);

            Assert.Equal(value, param.Value);
            Assert.Equal(direction, param.Direction);
            Assert.Equal(metadata, param.Metadata);
        }

        [Theory, CustomAutoData]
        public void IParam_Value(bool? value)
        {
            var param = new ParamBoolean(value, ParameterDirection.Input);

            IParam<bool> bolParam = param;
            Assert.NotNull(bolParam);
            Assert.Equal(value ?? false, bolParam.Value);
        }

        [Fact]
        public void GetDeclareLine_Input_ReturnsNull()
        {
            var param = new ParamBoolean(true, ParameterDirection.Input);

            Assert.Null(param.GetDeclareLine());
        }

        [Theory, CustomAutoData]
        internal void GetDeclareLine_Ouput_CallsMetadata(Mock<MetadataOracleBoolean> metadata, string declareExpected, string name)
        {
            metadata.Setup(m => m.GetDeclareLine(name))
                .Returns(declareExpected)
                .Verifiable();

            var param = new ParamBoolean(null, ParameterDirection.Output, metadata.Object);

            param.SetParameterName(name);

            var declare = param.GetDeclareLine();

            Assert.Equal(declareExpected, declare);
            metadata.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetDeclareLine_InputOuput_CallsMetadata(Mock<MetadataOracleBoolean> metadata, bool? value, string declareExpected, string name)
        {
            metadata.Setup(m => m.GetDeclareLine(name))
                .Returns(declareExpected)
                .Verifiable();

            var param = new ParamBoolean(value, ParameterDirection.InputOutput, metadata.Object);

            param.SetParameterName(name);

            var declare = param.GetDeclareLine();

            Assert.Equal(declareExpected, declare);
            metadata.Verify();
        }

        [Theory, CustomAutoData]
        internal void SetOutputValue_SetsDecimalValue(Mock<MetadataOracleBoolean> metadata, OracleDecimal value)
        {
            metadata.Setup(m => m.GetBooleanValue(value))
                .Returns(true)
                .Verifiable();

            var param = new ParamBoolean(false, ParameterDirection.InputOutput, metadata.Object);

            param.SetOutputValue(value);

            Assert.Equal(true, param.Value);
            metadata.Verify();
        }

        [Fact]
        internal void SetOutputValue_InvalidInput_DoesNotSetValue()
        {
            var metadata = new Mock<MetadataOracleBoolean>(MockBehavior.Strict);

            var param = new ParamBoolean(false, ParameterDirection.InputOutput, metadata.Object);

            param.SetOutputValue(null);

            Assert.Equal(false,  param.Value);
            metadata.Verify();
        }

        [Theory, CustomAutoData]
        internal async Task SetOutputValueAsync_SetsDecimalValue(Mock<MetadataOracleBoolean> metadata, OracleDecimal value)
        {
            metadata.Setup(m => m.GetBooleanValue(value))
                .Returns(true)
                .Verifiable();

            var param = new ParamBoolean(false, ParameterDirection.InputOutput, metadata.Object);

            await param.SetOutputValueAsync(value);

            Assert.Equal(true, param.Value);
            metadata.Verify();
        }

        [Fact]
        internal async Task SetOutputValueAsync_InvalidInput_DoesNotSetValue()
        {
            var metadata = new Mock<MetadataOracleBoolean>(MockBehavior.Strict);

            var param = new ParamBoolean(false, ParameterDirection.InputOutput, metadata.Object);

            await param.SetOutputValueAsync(null);

            Assert.Equal(false, param.Value);
            metadata.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameter_OutputParameter_CallsMetadata(int startNumber, Mock<MetadataOracleBoolean> metadata, bool? value, OracleParameter expectedParameter)
        {
            metadata.Setup(m => m.GetOracleParameter(value, startNumber))
                .Returns(expectedParameter)
                .Verifiable();

            var param = new ParamBoolean(value, ParameterDirection.Output, metadata.Object);

            var oracleParam = param.GetOracleParameter(startNumber);

            Assert.NotNull(oracleParam);
            Assert.Equal(expectedParameter, oracleParam);
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameter_InputOutputParameter_CallsMetadata(int startNumber, Mock<MetadataOracleBoolean> metadata, bool? value, OracleParameter expectedParameter)
        {
            metadata.Setup(m => m.GetOracleParameter(value, startNumber))
                .Returns(expectedParameter)
                .Verifiable();

            var param = new ParamBoolean(value, ParameterDirection.InputOutput, metadata.Object);

            var oracleParam = param.GetOracleParameter(startNumber);

            Assert.NotNull(oracleParam);
            Assert.Equal(expectedParameter, oracleParam);
            metadata.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameter_InputParameter_CallsMetadata(int startNumber, bool? value, string name)
        {
            var metadata = new Mock<MetadataOracleBoolean>(MockBehavior.Strict);

            var param = new ParamBoolean(value, ParameterDirection.Input, metadata.Object);

            param.SetParameterName(name);

            var oracleParam = param.GetOracleParameter(startNumber);

            Assert.NotNull(oracleParam);
            Assert.Equal(name, oracleParam.ParameterName);
            Assert.Equal(value, oracleParam.Value);
        }

        [Theory, CustomAutoData]
        internal void PrepareOutputParameter(int startNumber, Mock<MetadataOracleBoolean> metadata, bool? value, string name, string outputStr, OracleParameter expectedParameter)
        {
            metadata.Setup(m => m.OutputString(startNumber, name))
                .Returns(outputStr)
                .Verifiable();

            metadata.Setup(m => m.GetOracleParameter(value, startNumber))
                .Returns(expectedParameter);

            var param = new ParamBoolean(value, ParameterDirection.Output, metadata.Object);

            param.SetParameterName(name);
            var prepared = param.PrepareOutputParameter(startNumber);

            metadata.Verify();
            Assert.NotNull(prepared);
            Assert.Equal(outputStr, prepared.OutputString);
            Assert.Equal(expectedParameter, prepared.OracleParameter);
            Assert.Equal(param, prepared.Parameter);
        }
    }
}
