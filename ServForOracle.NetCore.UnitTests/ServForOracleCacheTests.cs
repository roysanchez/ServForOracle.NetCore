﻿using AutoFixture.Xunit2;
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
using ServForOracle.NetCore.Extensions;

namespace ServForOracle.NetCore.UnitTests
{
    public class ServForOracleCacheTests
    {
        public class TestClass
        {

        }
        UDTInfoAttributeTests udtInfoAttributeTests = new UDTInfoAttributeTests();

        public ServForOracleCacheTests()
        {
            
        }

        [Fact]
        public void Create_NullCache_ThrowsInvalidArgument()
        {
            ResetStaticCacheField();
            Assert.Throws<ArgumentNullException>("memoryCache", () => ServForOracleCache.Create(null));
        }

        private void ResetStaticCacheField()
        {
            var field = typeof(ServForOracleCache).GetField("_servForOracleCache", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(null, null);
        }

        [Theory, CustomAutoData]
        public void Create_IsSingleton(IMemoryCache memoryCache, IMemoryCache memoryCache2)
        {
            ResetStaticCacheField();
            var cache1 = ServForOracleCache.Create(memoryCache);
            var cache2 = ServForOracleCache.Create(memoryCache2);

            Assert.Equal(cache1, cache2);
            Assert.Equal(memoryCache, cache2.Cache);
        }


        [Theory, CustomAutoData]
        internal void CreateSaveUdtInfo_CreatesEntryInCache(Mock<IMemoryCache> memoryCache, Mock<ICacheEntry> entry, string name, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            ResetStaticCacheField();
            var cache = ServForOracleCache.Create(memoryCache.Object);

            memoryCache.Setup(m => m.CreateEntry($"udt-{name}")).Returns(entry.Object);
            
            entry.SetupSet((e) => e.Value = (info, props, fuzzyNameMatch)).Verifiable();

            cache.SaveUdtInfo(name, info, props, fuzzyNameMatch);

            entry.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetOtherUdtInfo_GetsEntryInCache(Mock<IMemoryCache> memoryCache, string name, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            ResetStaticCacheField();
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
            ResetStaticCacheField();
            var cache = ServForOracleCache.Create(memoryCache.Object);

            (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) expectedValue = default;
            object validParameter = null;
            memoryCache.Setup(m => m.TryGetValue($"udt-{name}", out validParameter))
                .Returns(false);

            var actualValue = cache.GetOtherUdtInfo(name);

            Assert.Equal(expectedValue, actualValue);
        }

        [Theory, CustomAutoData]
        internal void PresetGetValueOrDefault_IsCollection(Mock<Type> typeMock, Mock<Type> type, Mock<IMemoryCache> memoryMock, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch, string fullName)
        {
            ResetStaticCacheField();
            var expectedValue = (info, props, fuzzyNameMatch);
            object validParameter = expectedValue;

            typeMock.SetReturnsDefault(true);
            typeMock.Setup(t => t.GetGenericArguments()).Returns(new[] { type.Object });

            type.Setup(t => t.FullName).Returns(fullName);
            memoryMock.Setup(m => m.TryGetValue($"udt-{fullName}", out validParameter)).Returns(true);

            var cache = ServForOracleCache.Create(memoryMock.Object);

            var actualValue = cache.PresetGetValueOrDefault(typeMock.Object);

            Assert.Equal(expectedValue, actualValue);
        }

        [Theory, CustomAutoData]
        internal void PresetGetValueOrDefault_IsCollection_Default(Mock<Type> typeMock, Mock<Type> type, Mock<IMemoryCache> memoryMock, string fullName)
        {
            ResetStaticCacheField();
            (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) expectedValue = default;
            object validParameter = null;

            typeMock.SetReturnsDefault(true);
            typeMock.Setup(t => t.GetGenericArguments()).Returns(new[] { type.Object });

            type.Setup(t => t.FullName).Returns(fullName);
            memoryMock.Setup(m => m.TryGetValue($"udt-{fullName}", out validParameter)).Returns(true);

            var cache = ServForOracleCache.Create(memoryMock.Object);

            var actualValue = cache.PresetGetValueOrDefault(typeMock.Object);

            Assert.Equal(expectedValue, actualValue);
        }

        [Theory, CustomAutoData]
        internal void PresetGetValueOrDefault_IsObject(Mock<IMemoryCache> memoryMock, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            ResetStaticCacheField();
            var expectedValue = (info, props, fuzzyNameMatch);
            object validParameter = expectedValue;
            var type = typeof(TestClass);

            memoryMock.Setup(m => m.TryGetValue($"udt-{type.FullName}", out validParameter)).Returns(true);

            var cache = ServForOracleCache.Create(memoryMock.Object);

            var actualValue = cache.PresetGetValueOrDefault(type);

            Assert.Equal(expectedValue.fuzzyNameMatch, actualValue.FuzzyMatch);
            Assert.Equal(expectedValue.info, actualValue.Info);
            Assert.Equal(expectedValue.props, actualValue.Props);
        }

        [Theory, CustomAutoData]
        internal void PresetGetValueOrDefault_IsObject_Default(Mock<IMemoryCache> memoryMock, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            ResetStaticCacheField();
            var expectedValue = (info, props, fuzzyNameMatch);
            object validParameter = expectedValue;
            var type = typeof(TestClass);

            memoryMock.Setup(m => m.TryGetValue($"udt-{type.FullName}", out validParameter)).Returns(true);

            var cache = ServForOracleCache.Create(memoryMock.Object);

            var actualValue = cache.PresetGetValueOrDefault(type);

            Assert.Equal(expectedValue, actualValue);
        }

        [Theory, CustomAutoData]
        internal void AddOracleUDTPresets_CreatesEntryInCache(Mock<IMemoryCache> memoryCache, Mock<ICacheEntry> entry, Mock<Type> type, string fullName, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            ResetStaticCacheField();
            var cache = ServForOracleCache.Create(memoryCache.Object);

            memoryCache.Setup(m => m.CreateEntry($"udt-{fullName}")).Returns(entry.Object);
            type.Setup(t => t.FullName).Returns(fullName);

            entry.SetupSet((e) => e.Value = (info, props, fuzzyNameMatch)).Verifiable();

            cache.AddOracleUDTPresets(type.Object, info, props, fuzzyNameMatch);

            entry.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetMetadata_ExistsInCache(MetadataOracle metadata, string name, Mock<IMemoryCache> memoryCache)
        {
            ResetStaticCacheField();
            object validParameter = metadata;
            memoryCache.Setup(m => m.TryGetValue($"metadata-{name}", out validParameter)).Returns(true);

            var cache = ServForOracleCache.Create(memoryCache.Object);

            var actual = cache.GetMetadata(name);

            Assert.Equal(metadata, actual);
        }

        [Theory, CustomAutoData]
        internal void GetMetadata_DoesNoExistsInCache_ReturnsDefault(string name, Mock<IMemoryCache> memoryCache)
        {
            ResetStaticCacheField();
            object validParameter = null;
            MetadataOracle metadata = default;

            memoryCache.Setup(m => m.TryGetValue($"metadata-{name}", out validParameter)).Returns(false);

            var cache = ServForOracleCache.Create(memoryCache.Object);

            var actual = cache.GetMetadata(name);

            Assert.Equal(metadata, actual);
        }

        [Theory, CustomAutoData]
        internal void SaveMetadata_CreatesEntryInCache(Mock<IMemoryCache> memoryCache, Mock<ICacheEntry> entry, string name, MetadataOracle metadata)
        {
            ResetStaticCacheField();
            var cache = ServForOracleCache.Create(memoryCache.Object);

            memoryCache.Setup(m => m.CreateEntry($"metadata-{name}")).Returns(entry.Object);

            entry.SetupSet((e) => e.Value = metadata).Verifiable();

            cache.SaveMetadata(name, metadata);

            entry.Verify();
        }

        [Theory, CustomAutoData]
        internal void SaveTypeDefinition_CreatesEntryInCache(Mock<IMemoryCache> memoryCache, Mock<ICacheEntry> entry, MetadataOracleTypeDefinition definition)
        {
            ResetStaticCacheField();
            var cache = ServForOracleCache.Create(memoryCache.Object);

            memoryCache.Setup(m => m.CreateEntry($"def-{definition.UDTInfo.FullObjectName}")).Returns(entry.Object);

            entry.SetupSet((e) => e.Value = definition).Verifiable();

            cache.SaveTypeDefinition(definition);

            entry.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetTypeDefinition_ExistsInCache(Mock<IMemoryCache> memoryCache, MetadataOracleTypeDefinition definition)
        {
            ResetStaticCacheField();
            object validParameter = definition;

            memoryCache.Setup(m => m.TryGetValue($"def-{definition.UDTInfo.FullObjectName}", out validParameter)).Returns(true);

            var cache = ServForOracleCache.Create(memoryCache.Object);

            var actual = cache.GetTypeDefinition(definition.UDTInfo.FullObjectName);

            Assert.Equal(definition, actual);
        }

        [Theory, CustomAutoData]
        internal void GetTypeDefinition_DoesNotExistsInCache_ReturnsDefault(Mock<IMemoryCache> memoryCache, string name)
        {
            ResetStaticCacheField();
            object validParameter = null;
            MetadataOracleTypeDefinition definition = default;

            memoryCache.Setup(m => m.TryGetValue($"def-{name}", out validParameter)).Returns(false);

            var cache = ServForOracleCache.Create(memoryCache.Object);

            var actual = cache.GetTypeDefinition(name);

            Assert.Equal(definition, actual);
        }

        [Theory, CustomAutoData]
        internal void GetUdtInfoFromAttributeOrPresetCache_FromAttribute_IsCollection(IMemoryCache memory, Mock<Type> colTypeMock, string schema, string objectName, string collectionSchema, string collectionName)
        {
            ResetStaticCacheField();
            colTypeMock.SetReturnsDefault(true);
            
            var type = udtInfoAttributeTests.GetTypeWithAttribute(schema, objectName, collectionSchema, collectionName);

            colTypeMock.Setup(t => t.GetGenericArguments()).Returns(new[] { type });

            var cache = ServForOracleCache.Create(memory);

            var udtInfo = cache.GetUdtInfoFromAttributeOrPresetCache(colTypeMock.Object);

            Assert.NotNull(udtInfo);
            Assert.Equal(schema, udtInfo.ObjectSchema, ignoreCase: true);
            Assert.Equal(objectName, udtInfo.ObjectName, ignoreCase: true);
            Assert.Equal(collectionSchema, udtInfo.CollectionSchema, ignoreCase: true);
            Assert.Equal(collectionName, udtInfo.CollectionName, ignoreCase: true);
        }

        [Theory, CustomAutoData]
        internal void GetUdtInfoFromAttributeOrPresetCache_FromCache_IsCollection(Mock<IMemoryCache> memoryMock, Type type, OracleUdtInfo info)
        {
            ResetStaticCacheField();
            var colType = type.CreateListType();

            (OracleUdtInfo Info, UdtPropertyNetPropertyMap[], bool) expectedValue = (info, default, default);
            object validParameter = expectedValue;

            memoryMock.Setup(m => m.TryGetValue($"udt-{type.FullName}", out validParameter))
                .Returns(true);

            var cache = ServForOracleCache.Create(memoryMock.Object);

            var udtInfo = cache.GetUdtInfoFromAttributeOrPresetCache(colType);

            Assert.NotNull(udtInfo);
            Assert.Equal(expectedValue.Info, udtInfo);
        }


        [Theory, CustomAutoData]
        internal void GetUdtInfoFromAttributeOrPresetCache_FromCache_IsObject(Mock<IMemoryCache> memoryMock, Type type, OracleUdtInfo info)
        {
            ResetStaticCacheField();
            (OracleUdtInfo Info, UdtPropertyNetPropertyMap[], bool) expectedValue = (info, default, default);
            object validParameter = expectedValue;

            memoryMock.Setup(m => m.TryGetValue($"udt-{type.FullName}", out validParameter))
                .Returns(true);

            var cache = ServForOracleCache.Create(memoryMock.Object);

            var udtInfo = cache.GetUdtInfoFromAttributeOrPresetCache(type);

            Assert.NotNull(udtInfo);
            Assert.Equal(expectedValue.Info, udtInfo);
        }

        [Theory, CustomAutoData]
        internal void GetUdtInfoFromAttributeOrPresetCache_FromAttribute_IsObject(IMemoryCache memory, string schema, string objectName, string collectionSchema, string collectionName)
        {
            ResetStaticCacheField();
            var type = udtInfoAttributeTests.GetTypeWithAttribute(schema, objectName, collectionSchema, collectionName);

            var cache = ServForOracleCache.Create(memory);

            var udtInfo = cache.GetUdtInfoFromAttributeOrPresetCache(type);

            Assert.NotNull(udtInfo);
            Assert.Equal(schema, udtInfo.ObjectSchema, ignoreCase: true);
            Assert.Equal(objectName, udtInfo.ObjectName, ignoreCase: true);
            Assert.Equal(collectionSchema, udtInfo.CollectionSchema, ignoreCase: true);
            Assert.Equal(collectionName, udtInfo.CollectionName, ignoreCase: true);
        }
    }
}
