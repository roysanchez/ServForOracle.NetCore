using Microsoft.Extensions.Logging;
using Moq;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class MetadataBuilderFactoryTests
    {
        [Theory, CustomAutoData]
        internal void Constructor_ReturnsMetadataBuilder(Mock<DbConnection> connection, string connectionString, ServForOracleCache cache, ILogger logger)
        {
            connection.SetupGet(c => c.ConnectionString).Returns(connectionString);
            var factory = new MetadataBuilderFactory(cache, logger);

            var builder = factory.CreateBuilder(connection.Object);

            Assert.NotNull(builder);
            Assert.Equal(connection.Object, builder.OracleConnection);
        }
    }
}
