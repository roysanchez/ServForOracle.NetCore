using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class OracleDbConnectionFactoryTests
    {
        [Fact]
        public void Constructor_NullParameter_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("connectionString", () => new OracleDbConnectionFactory(null));
        }

        [Fact]
        public void CreateConnection_ReturnsNewOracleConnection()
        {
            var factory = new OracleDbConnectionFactory("Data Source=MyOracleDB;User Id=myUsername;Password=myPassword;");

            var connection = factory.CreateConnection();

            Assert.NotNull(connection);
            Assert.IsType<OracleConnection>(connection);
        }

        [Theory, CustomAutoData]
        public void CreateConnection_WrongConnectionString_WrongFormat_Throws(string wrongConnectionString)
        {
            var factory = new OracleDbConnectionFactory(wrongConnectionString);

            Assert.Throws<ArgumentException>(() => factory.CreateConnection());
        }

        [Fact]
        public void Dispose_WithCreatedConnection()
        {
            var factory = new OracleDbConnectionFactory("Data Source=MyOracleDB;User Id=myUsername;Password=myPassword;");

            var connection = factory.CreateConnection();

            factory.Dispose();
        }
    }
}
