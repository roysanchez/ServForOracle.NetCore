using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ServForOracleCacheTests
    {
        public ServForOracleCacheTests()
        {
            ResetStaticCacheField();
        }

        [Fact]
        public void Create_NullCache_ThrowsInvalidArgument()
        {
            Assert.Throws<ArgumentNullException>("memoryCache", () => ServForOracleCache.Create(null));
        }

        private void ResetStaticCacheField()
        {
            var field = typeof(ServForOracleCache).GetField("servForOracleCache", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(null, null);
        }

        [Theory, CustomAutoData]
        public void Create_IsSingleton(IMemoryCache memoryCache, IMemoryCache memoryCache2)
        {
            var cache1 = ServForOracleCache.Create(memoryCache);
            var cache2 = ServForOracleCache.Create(memoryCache2);

            Assert.Equal(cache1, cache2);
            Assert.Equal(memoryCache, cache2.Cache);
        }


        [Theory, CustomAutoData]
        internal void CreateSaveUdtInfo_CreatesEntryInCache(Mock<IMemoryCache> memoryCache, Mock<ICacheEntry> entry, string name, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            var cache = ServForOracleCache.Create(memoryCache.Object);

            memoryCache.Setup(m => m.CreateEntry($"udt-{name}")).Returns(entry.Object);
            
            entry.SetupSet((e) => e.Value = (info, props, fuzzyNameMatch)).Verifiable();

            cache.SaveUdtInfo(name, info, props, fuzzyNameMatch);

            entry.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetOtherUdtInfo_GetsEntryInCache(Mock<IMemoryCache> memoryCache, string name, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            var cache = ServForOracleCache.Create(memoryCache.Object);

            var expectedValue = (info, props, fuzzyNameMatch);
            object validParameter = expectedValue;
            memoryCache.Setup(m => m.TryGetValue($"udt-{name}", out validParameter))
                .Returns(true);
            
            var actualValue = cache.GetOtherUdtInfo(name);
            
            Assert.Equal(expectedValue, actualValue);
        }

        [Theory, CustomAutoData]
        internal void GetOtherUdtInfo_GetsEntryNotInCache_ReturnsDefault(Mock<IMemoryCache> memoryCache, string name)
        {
            var cache = ServForOracleCache.Create(memoryCache.Object);

            (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) expectedValue = default;
            object validParameter = null;
            memoryCache.Setup(m => m.TryGetValue($"udt-{name}", out validParameter))
                .Returns(false);

            var actualValue = cache.GetOtherUdtInfo(name);

            Assert.Equal(expectedValue, actualValue);
        }
    }
}
