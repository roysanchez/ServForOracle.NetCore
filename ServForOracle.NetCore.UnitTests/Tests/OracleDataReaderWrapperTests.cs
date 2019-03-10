using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.UnitTests.Config;
using ServForOracle.NetCore.Wrapper;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class OracleDataReaderWrapperTests
    {
        [Fact]
        public void Constructor_NullParameter_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new OracleDataReaderWrapper(null));
        }

        [Fact]
        public void Read_Throws()
        {
            var dataReader = (OracleDataReader)FormatterServices.GetUninitializedObject(typeof(OracleDataReader));
            var readerWrapper = new OracleDataReaderWrapper(dataReader);

            Assert.ThrowsAny<Exception>(() => readerWrapper.Read());
        }

        [Fact]
        public void ReadAsync_Throws()
        {
            var dataReader = (OracleDataReader)FormatterServices.GetUninitializedObject(typeof(OracleDataReader));
            var readerWrapper = new OracleDataReaderWrapper(dataReader);

            Assert.ThrowsAnyAsync<Exception>(() => readerWrapper.ReadAsync());
        }

        [Theory, CustomAutoData]
        public void GetOracleValue_Throws(int i)
        {
            var dataReader = (OracleDataReader)FormatterServices.GetUninitializedObject(typeof(OracleDataReader));
            var readerWrapper = new OracleDataReaderWrapper(dataReader);

            Assert.ThrowsAny<Exception>(() => readerWrapper.GetOracleValue(i));
        }
    }
}
