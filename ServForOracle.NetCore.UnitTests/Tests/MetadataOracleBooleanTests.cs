using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class MetadataOracleBooleanTests
    {
        [Theory, CustomAutoData]
        public void GetDeclareLine_Works(string parameterName)
        {
            var metadataBoolean = new MetadataOracleBoolean();

            var actual = metadataBoolean.GetDeclareLine(parameterName);

            Assert.Equal($"{parameterName} boolean;", actual);
        }

        [Fact]
        public void GetDeclareLine_ThrowsNullArgument()
        {
            var metadataBoolean = new MetadataOracleBoolean();

            Assert.Throws<ArgumentNullException>("parameterName", () => metadataBoolean.GetDeclareLine(null));
        }


        [Theory, CustomAutoData]
        public void OutputString_Works(int startNumber, string fieldName)
        {
            var metadataBoolean = new MetadataOracleBoolean();

            var actual = metadataBoolean.OutputString(startNumber, fieldName);

            Assert.Equal($@"
                if({fieldName}) then
                    :{startNumber} := 1;
                else
                    :{startNumber} := 0;
                end if;", actual);
        }

        [Fact]
        public void OutputString_ThrowsNullArgument()
        {
            var metadataBoolean = new MetadataOracleBoolean();

            Assert.Throws<ArgumentNullException>("fieldName", () => metadataBoolean.OutputString(0, null));
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_Works(bool? expected, int counter)
        {
            var metadataBoolean = new MetadataOracleBoolean();

            var actual = metadataBoolean.GetOracleParameter(expected, counter);

            Assert.NotNull(actual);
            Assert.Equal($":{counter}", actual.ParameterName);
            Assert.Equal(expected.HasValue ? Convert.ToByte(expected.Value) : (byte?) null, actual.Value);
        }

        [Theory, CustomAutoData]
        public void GetBooleanValue_Works(bool value)
        {
            var parameter = new OracleDecimal(value ? 1 : 0);
            var metadataBoolean = new MetadataOracleBoolean();

            var actual = metadataBoolean.GetBooleanValue(parameter);

            Assert.NotNull(actual);
            Assert.IsType<bool>(actual);
        }

        [Theory, CustomAutoData]
        public void GetBooleanValue_NullOrInvalidType_ReturnsNull(string str)
        {
            var metadataBoolean = new MetadataOracleBoolean();

            Assert.Null(metadataBoolean.GetBooleanValue(str));
            Assert.Null(metadataBoolean.GetBooleanValue(null));
        }
    }
}
