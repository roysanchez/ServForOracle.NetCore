using AutoFixture.Xunit2;
using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Config;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ConfigurePresetMappingsTests
    {
        public class TestClass
        {
            public string Roy { get; set; }
        }

        [Theory, CustomAutoData]
        public void AddOracleUDT_ValidParameterWontCallsLogger(string objectSchema, string objectName, Mock<ServForOracleCache> servForOracleCache)
        {
            var test = new PresetMap<TestClass>(objectSchema, objectName);
            var logger = new Mock<ILogger<ConfigurePresetMappings>>(MockBehavior.Strict);
            
            var config = new ConfigurePresetMappings(logger.Object, servForOracleCache.Object);

            //Throws if log is called
            config.AddOracleUDT(test);
            
            logger.Verify(l => l.Log(LogLevel.Warning, 0, It.IsAny< FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()), Times.Never);
            servForOracleCache.Verify(m => m.AddOracleUDTPresets(test.Type, test.Info, test.ReplacedProperties, true), Times.Once);
        }

        [Theory, CustomAutoData]
        public void AddOracleUDT_NullParameterCallsLogger(string objectSchema, string objectName, Mock<ServForOracleCache> servForOracleCache)
        {
            var test = new PresetMap<TestClass>(objectSchema, objectName);
            var logger = new Mock<ILogger<ConfigurePresetMappings>>(MockBehavior.Strict);
            
            logger.Setup(l => l.Log(LogLevel.Warning, 0, It.IsAny<FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();
            
            var config = new ConfigurePresetMappings(logger.Object, servForOracleCache.Object);

            config.AddOracleUDT(null);

            logger.Verify();
            servForOracleCache.Verify(m => m.AddOracleUDTPresets(test.Type, test.Info, test.ReplacedProperties, true), Times.Never);
        }
    }
}
