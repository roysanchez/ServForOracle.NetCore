using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using ServForOracle.NetCore.UnitTests.TestTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{

    public class MetadataBuilderTests
    {
        Mock<TestDbCommand> cmdMoq;
        Mock<TestDbConnection> connectionMoq;
        Mock<ILogger> loggerMoq = new Mock<ILogger>();
        Mock<ServForOracleCache> cacheMoq = new Mock<ServForOracleCache>(new Mock<IMemoryCache>().Object);

        public class TestRoy
        {
            public string Name { get; set; }
        }

        public class SuperTestClass
        {
            public TestRoy TypeProperty { get; set; }
        }

        public class SuperCollectionClass
        {
            public TestRoy[] Array { get; set; }
        }

        public MetadataBuilderTests()
        {
            var fix = new Fixture();
            cmdMoq = new Mock<TestDbCommand>() { CallBase = true };
            connectionMoq = new Mock<TestDbConnection>() { CallBase = true };
            connectionMoq.SetupProperty(c => c.ConnectionString, fix.Create<string>());
        }

        [Fact]
        public void Constructor_ConnectionStringNull_Throws()
        {
            connectionMoq.Object.ConnectionString = null;
            Assert.Throws<ArgumentNullException>(() => new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object));
        }

        [Fact]
        public void Constructor_DbConnectionNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MetadataBuilder(null, cacheMoq.Object, loggerMoq.Object));
        }

        [Fact]
        public void Constructor_LoggerNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, null));
        }

        [Fact]
        public void Constructor_CacheNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MetadataBuilder(connectionMoq.Object, null, loggerMoq.Object));
        }

        #region async

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_FindsInCache(OracleUdtInfo info, MetadataOracleTypeDefinition typeDefinition, UdtPropertyNetPropertyMap[] properties)
        {
            var metadata = new MetadataOracleObject<TestRoy>(cacheMoq.Object, typeDefinition, properties, fuzzyNameMatch: true);

            cacheMoq.Setup(c => c.GetMetadata(typeof(TestRoy).FullName))
                .Returns(metadata)
                .Verifiable();

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = await builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(info);

            cacheMoq.Verify();
            Assert.Equal(metadata, result);
        }

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb_PlainType(OracleUdtInfo info, int order, string name,
             (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) preset)
        {
            var readerMoq = new Mock<DbDataReader>();

            connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);
            connectionMoq.Setup(c => c._CreateDbCommand())
                .Returns(cmdMoq.Object);

            cmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).ReturnsAsync(readerMoq.Object);
            readerMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
            readerMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            readerMoq.Setup(r => r.GetInt32(0)).Returns(order);
            readerMoq.Setup(r => r.GetString(1)).Returns(name);

            cacheMoq.Setup(c => c.SaveTypeDefinition(It.IsAny<MetadataOracleTypeDefinition>()));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(typeof(TestRoy)))
                .Returns(preset);

            MetadataOracle metadata = null;
            cacheMoq.Setup(c => c.SaveMetadata(typeof(TestRoy).FullName, It.IsAny<MetadataOracle>()))
                .Callback((string n, object meta) => metadata = meta as MetadataOracle);

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = await builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(info);

            cacheMoq.VerifyAll();
            readerMoq.VerifyAll();
            Assert.NotNull(result);
            Assert.NotNull(result.OracleTypeNetMetadata);

            Assert.NotNull(result.OracleTypeNetMetadata.Properties);
            var prop = Assert.Single(result.OracleTypeNetMetadata.Properties);
            Assert.Equal(order, prop.Order);
            Assert.Equal(name, prop.Name, ignoreCase: true);

            Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
            Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

            Assert.IsType<MetadataOracleObject<TestRoy>>(metadata);
            Assert.Equal(metadata, result);
        }

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb_Object(OracleUdtInfo info, string schema, string objectName, int[] propOrder, string[] propName)
        {
            var superType = typeof(SuperTestClass);
            var subtype = typeof(TestRoy);

            var subCmdMoq = new Mock<TestDbCommand>() { CallBase = true };
            var subReaderMoq = new Mock<DbDataReader>();
            var readerMoq = new Mock<DbDataReader>();
            var propMap = new[] { new UdtPropertyNetPropertyMap(nameof(SuperTestClass.TypeProperty), propName[0]) };
            var subPropMap = new[] { new UdtPropertyNetPropertyMap(nameof(TestRoy.Name), propName[1]) };

            connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);

            cmdMoq.Object.Connection = connectionMoq.Object;
            subCmdMoq.Object.Connection = connectionMoq.Object;
            connectionMoq.SetupSequence(c => c._CreateDbCommand())
                .Returns(cmdMoq.Object)
                .Returns(subCmdMoq.Object);

            cmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).ReturnsAsync(readerMoq.Object);
            readerMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            readerMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            readerMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[0]);
            readerMoq.Setup(r => r.GetString(1)).Returns(propName[0]);
            readerMoq.Setup(r => r.GetString(2)).Returns(schema);
            readerMoq.Setup(r => r.GetString(3)).Returns(objectName);
            readerMoq.Setup(r => r.GetString(4)).Returns("OBJECT");

            cacheMoq.Setup(c => c.SaveTypeDefinition(It.IsAny<MetadataOracleTypeDefinition>()));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(superType)).Returns((null, propMap, FuzzyMatch: true));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(subtype)).Returns((null, subPropMap, FuzzyMatch: true));

            MetadataOracle metadata = null;
            cacheMoq.Setup(c => c.SaveMetadata(superType.FullName, It.IsAny<MetadataOracle>()))
                .Callback((string n, object meta) => metadata = meta as MetadataOracle);


            subCmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subReaderMoq.Object);

            subReaderMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            subReaderMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            subReaderMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[1]);
            subReaderMoq.Setup(r => r.GetString(1)).Returns(propName[1]);


            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = await builder.GetOrRegisterMetadataOracleObjectAsync<SuperTestClass>(info);

            cacheMoq.VerifyAll();
            readerMoq.VerifyAll();
            Assert.NotNull(result);
            Assert.NotNull(result.OracleTypeNetMetadata);
            Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
            Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

            Assert.NotNull(result.OracleTypeNetMetadata.Properties);
            var prop = Assert.Single(result.OracleTypeNetMetadata.Properties);

            Assert.Equal(propOrder[0], prop.Order);
            Assert.Equal(propName[0], prop.Name, ignoreCase: true);
            Assert.NotNull(prop.PropertyMetadata);
            Assert.NotNull(prop.NETProperty);
            Assert.Equal(superType.GetProperties()[0], prop.NETProperty);

            Assert.NotNull(prop.PropertyMetadata.Properties);
            var subProp = Assert.Single(prop.PropertyMetadata.Properties);
            Assert.NotNull(subProp);

            Assert.Equal(propOrder[1], subProp.Order);
            Assert.Equal(propName[1], subProp.Name, ignoreCase: true);
            Assert.NotNull(subProp.NETProperty);
            Assert.Equal(subtype.GetProperties()[0], subProp.NETProperty);
            Assert.Null(subProp.PropertyMetadata);
        }

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb_Collection(OracleUdtInfo info, string schema, string objectName, string subObjectName, int[] propOrder, string[] propName)
        {
            var superType = typeof(SuperCollectionClass);
            var subtype = typeof(TestRoy);

            var collectionCmdMoq = new Mock<TestDbCommand>() { CallBase = true };
            var subCmdMoq = new Mock<TestDbCommand>() { CallBase = true };

            var collectionReaderMoq = new Mock<DbDataReader>();
            var subReaderMoq = new Mock<DbDataReader>();
            var readerMoq = new Mock<DbDataReader>();

            var propMap = new[] { new UdtPropertyNetPropertyMap(nameof(SuperCollectionClass.Array), propName[0]) };
            var subPropMap = new[] { new UdtPropertyNetPropertyMap(nameof(TestRoy.Name), propName[1]) };

            connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);

            cmdMoq.Object.Connection = connectionMoq.Object;
            subCmdMoq.Object.Connection = connectionMoq.Object;
            connectionMoq.SetupSequence(c => c._CreateDbCommand())
                .Returns(cmdMoq.Object)
                .Returns(collectionCmdMoq.Object)
                .Returns(subCmdMoq.Object);

            cmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).ReturnsAsync(readerMoq.Object);
            readerMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            readerMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            readerMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[0]);
            readerMoq.Setup(r => r.GetString(1)).Returns(propName[0]);
            readerMoq.Setup(r => r.GetString(2)).Returns(schema);
            readerMoq.Setup(r => r.GetString(3)).Returns(objectName);
            readerMoq.Setup(r => r.GetString(4)).Returns("COLLECTION");

            cacheMoq.Setup(c => c.SaveTypeDefinition(It.IsAny<MetadataOracleTypeDefinition>()));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(superType)).Returns((null, propMap, FuzzyMatch: true));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(subtype)).Returns((null, subPropMap, FuzzyMatch: true));

            MetadataOracle metadata = null;
            cacheMoq.Setup(c => c.SaveMetadata(superType.FullName, It.IsAny<MetadataOracle>()))
                .Callback((string n, object meta) => metadata = meta as MetadataOracle);

            collectionCmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(collectionReaderMoq.Object);

            collectionReaderMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            collectionReaderMoq.Setup(r => r.GetString(0)).Returns(schema);
            collectionReaderMoq.Setup(r => r.GetString(1)).Returns(subObjectName);

            subCmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subReaderMoq.Object);

            subReaderMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            subReaderMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            subReaderMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[1]);
            subReaderMoq.Setup(r => r.GetString(1)).Returns(propName[1]);


            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = await builder.GetOrRegisterMetadataOracleObjectAsync<SuperCollectionClass>(info);

            cacheMoq.VerifyAll();
            readerMoq.VerifyAll();
            Assert.NotNull(result);
            Assert.NotNull(result.OracleTypeNetMetadata);
            Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
            Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

            Assert.NotNull(result.OracleTypeNetMetadata.Properties);
            var prop = Assert.Single(result.OracleTypeNetMetadata.Properties);

            Assert.Equal(propOrder[0], prop.Order);
            Assert.Equal(propName[0], prop.Name, ignoreCase: true);
            Assert.NotNull(prop.PropertyMetadata);
            Assert.NotNull(prop.NETProperty);
            Assert.Equal(superType.GetProperties()[0], prop.NETProperty);

            Assert.NotNull(prop.PropertyMetadata.Properties);
            var subProp = Assert.Single(prop.PropertyMetadata.Properties);
            Assert.NotNull(subProp);

            Assert.Equal(propOrder[1], subProp.Order);
            Assert.Equal(propName[1], subProp.Name, ignoreCase: true);
            Assert.NotNull(subProp.NETProperty);
            Assert.Equal(subtype.GetProperties()[0], subProp.NETProperty);
            Assert.Null(subProp.PropertyMetadata);
        }

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb_Collection_InvalidSub_Throws(OracleUdtInfo info, string schema, string objectName, int[] propOrder, string[] propName, string dataSource)
        {
            var superType = typeof(SuperCollectionClass);
            var subtype = typeof(TestRoy);

            var collectionCmdMoq = new Mock<TestDbCommand>() { CallBase = true };

            var collectionReaderMoq = new Mock<DbDataReader>();
            var readerMoq = new Mock<DbDataReader>();

            var propMap = new[] { new UdtPropertyNetPropertyMap(nameof(SuperCollectionClass.Array), propName[0]) };
            var subPropMap = new[] { new UdtPropertyNetPropertyMap(nameof(TestRoy.Name), propName[1]) };

            connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);
            connectionMoq.Object._DataSource = dataSource;

            cmdMoq.Object.Connection = connectionMoq.Object;
            connectionMoq.SetupSequence(c => c._CreateDbCommand())
                .Returns(cmdMoq.Object)
                .Returns(collectionCmdMoq.Object);

            cmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(readerMoq.Object);

            readerMoq.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            readerMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            readerMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[0]);
            readerMoq.Setup(r => r.GetString(1)).Returns(propName[0]);
            readerMoq.Setup(r => r.GetString(2)).Returns(schema);
            readerMoq.Setup(r => r.GetString(3)).Returns(objectName);
            readerMoq.Setup(r => r.GetString(4)).Returns("COLLECTION");

            collectionCmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(collectionReaderMoq.Object);

            collectionReaderMoq.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() => builder.GetOrRegisterMetadataOracleObjectAsync<SuperCollectionClass>(info));

            Assert.NotNull(exception);
            Assert.Equal($"User connected to {dataSource} does not have permission to read the information about the collection type {schema}.{objectName}", exception.Message);

            cmdMoq.VerifyAll();
            readerMoq.VerifyAll();
            collectionCmdMoq.VerifyAll();
            collectionReaderMoq.VerifyAll();
        }

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_TypeDefinitionInCache(MetadataOracleTypeDefinition metadataOracleType)
        {
            var readerMoq = new Mock<DbDataReader>();
            var info = metadataOracleType.UDTInfo;
            var properties = metadataOracleType.Properties.ToArray();

            cacheMoq.Setup(c => c.GetTypeDefinition(info.FullObjectName)).Returns(metadataOracleType);

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var metadata = await builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(info);

            cmdMoq.VerifyAll();


            Assert.NotNull(metadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata);
            Assert.Equal(info, metadata.OracleTypeNetMetadata.UDTInfo);
            
            Assert.Collection(metadata.OracleTypeNetMetadata.Properties,
                p => Assert.Equal(properties[0].Name, p.Name),
                p => Assert.Equal(properties[1].Name, p.Name),
                p => Assert.Equal(properties[2].Name, p.Name));
        }


        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_NoUdt_Object_InAttributeOrPreset(MetadataOracleTypeDefinition metadataOracleType)
        {
            var readerMoq = new Mock<DbDataReader>();
            var info = metadataOracleType.UDTInfo;
            var properties = metadataOracleType.Properties.ToArray();
            var type = typeof(TestRoy);

            cacheMoq.Setup(c => c.GetTypeDefinition(info.FullObjectName)).Returns(metadataOracleType);
            cacheMoq.Setup(c => c.GetUdtInfoFromAttributeOrPresetCache(type)).Returns(info);

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var metadata = await builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(null);

            cmdMoq.VerifyAll();

            Assert.NotNull(metadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata);
            Assert.Equal(info, metadata.OracleTypeNetMetadata.UDTInfo);

            Assert.Collection(metadata.OracleTypeNetMetadata.Properties,
                p => Assert.Equal(properties[0].Name, p.Name),
                p => Assert.Equal(properties[1].Name, p.Name),
                p => Assert.Equal(properties[2].Name, p.Name));
        }

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_NoUdt_Object_NotInAttributeOrPreset_ThrowsArgumentException(MetadataOracleTypeDefinition metadataOracleType)
        {
            var readerMoq = new Mock<DbDataReader>();
            var info = metadataOracleType.UDTInfo;
            var properties = metadataOracleType.Properties.ToArray();
            var type = typeof(TestRoy);

            cacheMoq.Setup(c => c.GetTypeDefinition(info.FullObjectName)).Returns(metadataOracleType);

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(null));

            Assert.Equal($"The type {type.FullName} needs to have the {nameof(OracleUdtAttribute)} attribute set or pass the {nameof(OracleUdtInfo)} class to the execute method.", exception.Message);
        }

        #endregion async

        #region sync

        [Theory, CustomAutoData]
        internal void GetMetadataOracleObject_FindsInCache(OracleUdtInfo info, MetadataOracleTypeDefinition typeDefinition, UdtPropertyNetPropertyMap[] properties)
        {
            var metadata = new MetadataOracleObject<TestRoy>(cacheMoq.Object, typeDefinition, properties, fuzzyNameMatch: true);

            cacheMoq.Setup(c => c.GetMetadata(typeof(TestRoy).FullName))
                .Returns(metadata)
                .Verifiable();

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = builder.GetOrRegisterMetadataOracleObject<TestRoy>(info);

            cacheMoq.Verify();
            Assert.Equal(metadata, result);
        }

        [Theory, CustomAutoData]
        internal void GetMetadataOracleObject_NotInCache_CallsDb_PlainType(OracleUdtInfo info, int order, string name,
             (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) preset)
        {
            var readerMoq = new Mock<DbDataReader>();

            connectionMoq.Setup(c => c.Open())
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);
            connectionMoq.Setup(c => c._CreateDbCommand())
                .Returns(cmdMoq.Object);

            cmdMoq.Setup(c => c._ExecuteDbDataReader(It.IsAny<CommandBehavior>())).Returns(readerMoq.Object);
            readerMoq.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            readerMoq.Setup(r => r.IsDBNull(2)).Returns(true);
            readerMoq.Setup(r => r.GetInt32(0)).Returns(order);
            readerMoq.Setup(r => r.GetString(1)).Returns(name);

            cacheMoq.Setup(c => c.SaveTypeDefinition(It.IsAny<MetadataOracleTypeDefinition>()));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(typeof(TestRoy)))
                .Returns(preset);

            MetadataOracle metadata = null;
            cacheMoq.Setup(c => c.SaveMetadata(typeof(TestRoy).FullName, It.IsAny<MetadataOracle>()))
                .Callback((string n, object meta) => metadata = meta as MetadataOracle);

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = builder.GetOrRegisterMetadataOracleObject<TestRoy>(info);

            cacheMoq.VerifyAll();
            readerMoq.VerifyAll();
            Assert.NotNull(result);
            Assert.NotNull(result.OracleTypeNetMetadata);

            Assert.NotNull(result.OracleTypeNetMetadata.Properties);
            var prop = Assert.Single(result.OracleTypeNetMetadata.Properties);
            Assert.Equal(order, prop.Order);
            Assert.Equal(name, prop.Name, ignoreCase: true);

            Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
            Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

            Assert.IsType<MetadataOracleObject<TestRoy>>(metadata);
            Assert.Equal(metadata, result);
        }

        [Theory, CustomAutoData]
        internal void GetMetadataOracleObject_NotInCache_CallsDb_Object(OracleUdtInfo info, string schema, string objectName, int[] propOrder, string[] propName)
        {
            var superType = typeof(SuperTestClass);
            var subtype = typeof(TestRoy);

            var subCmdMoq = new Mock<TestDbCommand>() { CallBase = true };
            var subReaderMoq = new Mock<DbDataReader>();
            var readerMoq = new Mock<DbDataReader>();
            var propMap = new[] { new UdtPropertyNetPropertyMap(nameof(SuperTestClass.TypeProperty), propName[0]) };
            var subPropMap = new[] { new UdtPropertyNetPropertyMap(nameof(TestRoy.Name), propName[1]) };

            connectionMoq.Setup(c => c.Open())
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);

            cmdMoq.Object.Connection = connectionMoq.Object;
            subCmdMoq.Object.Connection = connectionMoq.Object;
            connectionMoq.SetupSequence(c => c._CreateDbCommand())
                .Returns(cmdMoq.Object)
                .Returns(subCmdMoq.Object);

            cmdMoq.Setup(c => c._ExecuteDbDataReader(It.IsAny<CommandBehavior>())).Returns(readerMoq.Object);
            readerMoq.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            readerMoq.Setup(r => r.IsDBNull(2))
                .Returns(false);

            readerMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[0]);
            readerMoq.Setup(r => r.GetString(1)).Returns(propName[0]);
            readerMoq.Setup(r => r.GetString(2)).Returns(schema);
            readerMoq.Setup(r => r.GetString(3)).Returns(objectName);
            readerMoq.Setup(r => r.GetString(4)).Returns("OBJECT");

            cacheMoq.Setup(c => c.SaveTypeDefinition(It.IsAny<MetadataOracleTypeDefinition>()));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(superType)).Returns((null, propMap, FuzzyMatch: true));
            cacheMoq.Setup(c => c.PresetGetValueOrDefault(subtype)).Returns((null, subPropMap, FuzzyMatch: true));

            MetadataOracle metadata = null;
            cacheMoq.Setup(c => c.SaveMetadata(superType.FullName, It.IsAny<MetadataOracle>()))
                .Callback((string n, object meta) => metadata = meta as MetadataOracle);


            subCmdMoq.Setup(c => c._ExecuteDbDataReader(It.IsAny<CommandBehavior>()))
                .Returns(subReaderMoq.Object);

            subReaderMoq.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            subReaderMoq.Setup(r => r.IsDBNull(2))
                .Returns(true);

            subReaderMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[1]);
            subReaderMoq.Setup(r => r.GetString(1)).Returns(propName[1]);


            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = builder.GetOrRegisterMetadataOracleObject<SuperTestClass>(info);

            cacheMoq.VerifyAll();
            readerMoq.VerifyAll();
            Assert.NotNull(result);
            Assert.NotNull(result.OracleTypeNetMetadata);
            Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
            Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

            Assert.NotNull(result.OracleTypeNetMetadata.Properties);
            var prop = Assert.Single(result.OracleTypeNetMetadata.Properties);

            Assert.Equal(propOrder[0], prop.Order);
            Assert.Equal(propName[0], prop.Name, ignoreCase: true);
            Assert.NotNull(prop.PropertyMetadata);
            Assert.NotNull(prop.NETProperty);
            Assert.Equal(superType.GetProperties()[0], prop.NETProperty);

            Assert.NotNull(prop.PropertyMetadata.Properties);
            var subProp = Assert.Single(prop.PropertyMetadata.Properties);
            Assert.NotNull(subProp);

            Assert.Equal(propOrder[1], subProp.Order);
            Assert.Equal(propName[1], subProp.Name, ignoreCase: true);
            Assert.NotNull(subProp.NETProperty);
            Assert.Equal(subtype.GetProperties()[0], subProp.NETProperty);
            Assert.Null(subProp.PropertyMetadata);
        }

        //[Theory, CustomAutoData]
        //internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb_Collection(OracleUdtInfo info, string schema, string objectName, string subObjectName, int[] propOrder, string[] propName)
        //{
        //    var superType = typeof(SuperCollectionClass);
        //    var subtype = typeof(TestRoy);

        //    var collectionCmdMoq = new Mock<TestDbCommand>() { CallBase = true };
        //    var subCmdMoq = new Mock<TestDbCommand>() { CallBase = true };

        //    var collectionReaderMoq = new Mock<DbDataReader>();
        //    var subReaderMoq = new Mock<DbDataReader>();
        //    var readerMoq = new Mock<DbDataReader>();

        //    var propMap = new[] { new UdtPropertyNetPropertyMap(nameof(SuperCollectionClass.Array), propName[0]) };
        //    var subPropMap = new[] { new UdtPropertyNetPropertyMap(nameof(TestRoy.Name), propName[1]) };

        //    connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
        //        .Returns(Task.CompletedTask)
        //        .Callback(() => connectionMoq.Object._State = ConnectionState.Open);

        //    cmdMoq.Object.Connection = connectionMoq.Object;
        //    subCmdMoq.Object.Connection = connectionMoq.Object;
        //    connectionMoq.SetupSequence(c => c._CreateDbCommand())
        //        .Returns(cmdMoq.Object)
        //        .Returns(collectionCmdMoq.Object)
        //        .Returns(subCmdMoq.Object);

        //    cmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).ReturnsAsync(readerMoq.Object);
        //    readerMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true)
        //        .ReturnsAsync(false);

        //    readerMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(false);

        //    readerMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[0]);
        //    readerMoq.Setup(r => r.GetString(1)).Returns(propName[0]);
        //    readerMoq.Setup(r => r.GetString(2)).Returns(schema);
        //    readerMoq.Setup(r => r.GetString(3)).Returns(objectName);
        //    readerMoq.Setup(r => r.GetString(4)).Returns("COLLECTION");

        //    cacheMoq.Setup(c => c.SaveTypeDefinition(It.IsAny<MetadataOracleTypeDefinition>()));
        //    cacheMoq.Setup(c => c.PresetGetValueOrDefault(superType)).Returns((null, propMap, FuzzyMatch: true));
        //    cacheMoq.Setup(c => c.PresetGetValueOrDefault(subtype)).Returns((null, subPropMap, FuzzyMatch: true));

        //    MetadataOracle metadata = null;
        //    cacheMoq.Setup(c => c.SaveMetadata(superType.FullName, It.IsAny<MetadataOracle>()))
        //        .Callback((string n, object meta) => metadata = meta as MetadataOracle);

        //    collectionCmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(collectionReaderMoq.Object);

        //    collectionReaderMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true)
        //        .ReturnsAsync(false);
        //    collectionReaderMoq.Setup(r => r.GetString(0)).Returns(schema);
        //    collectionReaderMoq.Setup(r => r.GetString(1)).Returns(subObjectName);

        //    subCmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(subReaderMoq.Object);

        //    subReaderMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true)
        //        .ReturnsAsync(false);

        //    subReaderMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);
        //    subReaderMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[1]);
        //    subReaderMoq.Setup(r => r.GetString(1)).Returns(propName[1]);


        //    var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

        //    var result = await builder.GetOrRegisterMetadataOracleObjectAsync<SuperCollectionClass>(info);

        //    cacheMoq.VerifyAll();
        //    readerMoq.VerifyAll();
        //    Assert.NotNull(result);
        //    Assert.NotNull(result.OracleTypeNetMetadata);
        //    Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
        //    Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

        //    Assert.NotNull(result.OracleTypeNetMetadata.Properties);
        //    var prop = Assert.Single(result.OracleTypeNetMetadata.Properties);

        //    Assert.Equal(propOrder[0], prop.Order);
        //    Assert.Equal(propName[0], prop.Name, ignoreCase: true);
        //    Assert.NotNull(prop.PropertyMetadata);
        //    Assert.NotNull(prop.NETProperty);
        //    Assert.Equal(superType.GetProperties()[0], prop.NETProperty);

        //    Assert.NotNull(prop.PropertyMetadata.Properties);
        //    var subProp = Assert.Single(prop.PropertyMetadata.Properties);
        //    Assert.NotNull(subProp);

        //    Assert.Equal(propOrder[1], subProp.Order);
        //    Assert.Equal(propName[1], subProp.Name, ignoreCase: true);
        //    Assert.NotNull(subProp.NETProperty);
        //    Assert.Equal(subtype.GetProperties()[0], subProp.NETProperty);
        //    Assert.Null(subProp.PropertyMetadata);
        //}

        //[Theory, CustomAutoData]
        //internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb_Collection_InvalidSub_Throws(OracleUdtInfo info, string schema, string objectName, int[] propOrder, string[] propName, string dataSource)
        //{
        //    var superType = typeof(SuperCollectionClass);
        //    var subtype = typeof(TestRoy);

        //    var collectionCmdMoq = new Mock<TestDbCommand>() { CallBase = true };

        //    var collectionReaderMoq = new Mock<DbDataReader>();
        //    var readerMoq = new Mock<DbDataReader>();

        //    var propMap = new[] { new UdtPropertyNetPropertyMap(nameof(SuperCollectionClass.Array), propName[0]) };
        //    var subPropMap = new[] { new UdtPropertyNetPropertyMap(nameof(TestRoy.Name), propName[1]) };

        //    connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
        //        .Returns(Task.CompletedTask)
        //        .Callback(() => connectionMoq.Object._State = ConnectionState.Open);
        //    connectionMoq.Object._DataSource = dataSource;

        //    cmdMoq.Object.Connection = connectionMoq.Object;
        //    connectionMoq.SetupSequence(c => c._CreateDbCommand())
        //        .Returns(cmdMoq.Object)
        //        .Returns(collectionCmdMoq.Object);

        //    cmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(readerMoq.Object);

        //    readerMoq.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    readerMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(false);

        //    readerMoq.Setup(r => r.GetInt32(0)).Returns(propOrder[0]);
        //    readerMoq.Setup(r => r.GetString(1)).Returns(propName[0]);
        //    readerMoq.Setup(r => r.GetString(2)).Returns(schema);
        //    readerMoq.Setup(r => r.GetString(3)).Returns(objectName);
        //    readerMoq.Setup(r => r.GetString(4)).Returns("COLLECTION");

        //    collectionCmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(collectionReaderMoq.Object);

        //    collectionReaderMoq.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(false);

        //    var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

        //    var exception = await Assert.ThrowsAsync<Exception>(() => builder.GetOrRegisterMetadataOracleObjectAsync<SuperCollectionClass>(info));

        //    Assert.NotNull(exception);
        //    Assert.Equal($"User connected to {dataSource} does not have permission to read the information about the collection type {schema}.{objectName}", exception.Message);

        //    cmdMoq.VerifyAll();
        //    readerMoq.VerifyAll();
        //    collectionCmdMoq.VerifyAll();
        //    collectionReaderMoq.VerifyAll();
        //}

        //[Theory, CustomAutoData]
        //internal async Task GetMetadataOracleObjectAsync_TypeDefinitionInCache(MetadataOracleTypeDefinition metadataOracleType)
        //{
        //    var readerMoq = new Mock<DbDataReader>();
        //    var info = metadataOracleType.UDTInfo;
        //    var properties = metadataOracleType.Properties.ToArray();

        //    cacheMoq.Setup(c => c.GetTypeDefinition(info.FullObjectName)).Returns(metadataOracleType);

        //    var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

        //    var metadata = await builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(info);

        //    cmdMoq.VerifyAll();


        //    Assert.NotNull(metadata);
        //    Assert.NotNull(metadata.OracleTypeNetMetadata);
        //    Assert.Equal(info, metadata.OracleTypeNetMetadata.UDTInfo);

        //    Assert.Collection(metadata.OracleTypeNetMetadata.Properties,
        //        p => Assert.Equal(properties[0].Name, p.Name),
        //        p => Assert.Equal(properties[1].Name, p.Name),
        //        p => Assert.Equal(properties[2].Name, p.Name));
        //}


        //[Theory, CustomAutoData]
        //internal async Task GetMetadataOracleObjectAsync_NoUdt_Object_InAttributeOrPreset(MetadataOracleTypeDefinition metadataOracleType)
        //{
        //    var readerMoq = new Mock<DbDataReader>();
        //    var info = metadataOracleType.UDTInfo;
        //    var properties = metadataOracleType.Properties.ToArray();
        //    var type = typeof(TestRoy);

        //    cacheMoq.Setup(c => c.GetTypeDefinition(info.FullObjectName)).Returns(metadataOracleType);
        //    cacheMoq.Setup(c => c.GetUdtInfoFromAttributeOrPresetCache(type)).Returns(info);

        //    var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

        //    var metadata = await builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(null);

        //    cmdMoq.VerifyAll();

        //    Assert.NotNull(metadata);
        //    Assert.NotNull(metadata.OracleTypeNetMetadata);
        //    Assert.Equal(info, metadata.OracleTypeNetMetadata.UDTInfo);

        //    Assert.Collection(metadata.OracleTypeNetMetadata.Properties,
        //        p => Assert.Equal(properties[0].Name, p.Name),
        //        p => Assert.Equal(properties[1].Name, p.Name),
        //        p => Assert.Equal(properties[2].Name, p.Name));
        //}

        //[Theory, CustomAutoData]
        //internal async Task GetMetadataOracleObjectAsync_NoUdt_Object_NotInAttributeOrPreset_ThrowsArgumentException(MetadataOracleTypeDefinition metadataOracleType)
        //{
        //    var readerMoq = new Mock<DbDataReader>();
        //    var info = metadataOracleType.UDTInfo;
        //    var properties = metadataOracleType.Properties.ToArray();
        //    var type = typeof(TestRoy);

        //    cacheMoq.Setup(c => c.GetTypeDefinition(info.FullObjectName)).Returns(metadataOracleType);

        //    var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

        //    var exception = await Assert.ThrowsAsync<ArgumentException>(() => builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(null));

        //    Assert.Equal($"The type {type.FullName} needs to have the {nameof(OracleUdtAttribute)} attribute set or pass the {nameof(OracleUdtInfo)} class to the execute method.", exception.Message);
        //}

        #endregion sync
    }
}
