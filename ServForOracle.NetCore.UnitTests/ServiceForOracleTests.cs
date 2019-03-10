using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ServiceForOracleTests
    {
        [Theory, CustomAutoData]
        public void Constructor_ThreeParameters_ConnectionString(ILogger<ServiceForOracle> logger, ServForOracleCache cache, string connectionString)
        {
            var service = new ServiceForOracle(logger, cache, connectionString);
        }

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameters_DbFactory(ILogger<ServiceForOracle> logger, ServForOracleCache cache, IDbConnectionFactory factory)
        {
            var service = new ServiceForOracle(logger, cache, factory);
        }

        [Theory, CustomAutoData]
        internal void Constructor_FourParameters(ILogger<ServiceForOracle> logger, ServForOracleCache cache, IDbConnectionFactory factory, MetadataOracleCommon common)
        {
            var service = new ServiceForOracle(logger, cache, factory, common);
        }

        [Theory, CustomAutoData]
        internal void Constructor_FourParameters_NullParameter_ThrowsArgumentNull(ILogger<ServiceForOracle> logger, ServForOracleCache cache, IDbConnectionFactory factory, MetadataOracleCommon common)
        {
            Assert.Throws<ArgumentNullException>("cache", () => new ServiceForOracle(logger, null, factory, common));
            Assert.Throws<ArgumentNullException>("factory", () => new ServiceForOracle(logger, cache, null, common));
            Assert.Throws<ArgumentNullException>("common", () => new ServiceForOracle(logger, cache, factory, null));
        }
    }
}
