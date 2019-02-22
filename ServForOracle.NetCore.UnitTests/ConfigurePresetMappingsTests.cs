using AutoFixture.Xunit2;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using ServForOracle.NetCore.Config;
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

        [Theory, AutoData]
        public void AddOracleUDT_ValidParameterWontCallsLogger(string objectSchema, string objectName)
        {
            var test = new PresetMap<TestClass>(objectSchema, objectName);
            var logger = new Mock<ILogger<ConfigurePresetMappings>>(MockBehavior.Strict);

            var config = new ConfigurePresetMappings(logger.Object);

            //Throws if log is called
            config.AddOracleUDT(test);
            
            logger.Verify(l => l.Log(LogLevel.Warning, 0, It.IsAny< FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()), Times.Never);
        }

        [Theory, AutoData]
        public void AddOracleUDT_NullParameterCallsLogger(string objectSchema, string objectName)
        {
            var test = new PresetMap<TestClass>(objectSchema, objectName);
            var logger = new Mock<ILogger<ConfigurePresetMappings>>(MockBehavior.Strict);

            logger.Setup(l => l.Log(LogLevel.Warning, 0, It.IsAny<FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();
            

            var config = new ConfigurePresetMappings(logger.Object);

            config.AddOracleUDT(null);

            logger.Verify();
        }
    }
}
