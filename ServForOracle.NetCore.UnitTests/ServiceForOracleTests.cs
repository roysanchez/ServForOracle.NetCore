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
        internal void Constructor_FourParameters(ILogger<ServiceForOracle> logger, ServForOracleCache cache, IDbConnectionFactory factory, MetadataOracleCommon common, IMetadataBuilderFactory builderFactory)
        {
            var service = new ServiceForOracle(logger, cache, factory, builderFactory, common);
        }

        [Theory, CustomAutoData]
        internal void Constructor_FourParameters_NullParameter_ThrowsArgumentNull(ILogger<ServiceForOracle> logger, ServForOracleCache cache, IDbConnectionFactory factory, MetadataOracleCommon common, IMetadataBuilderFactory builderFactory)
        {
            Assert.Throws<ArgumentNullException>("cache", () => new ServiceForOracle(logger, null, factory, builderFactory, common));
            Assert.Throws<ArgumentNullException>("factory", () => new ServiceForOracle(logger, cache, null, builderFactory, common));
            Assert.Throws<ArgumentNullException>("builderFactory", () => new ServiceForOracle(logger, cache, factory, null, common));
            Assert.Throws<ArgumentNullException>("common", () => new ServiceForOracle(logger, cache, factory, builderFactory, null));
        }
    }
}
