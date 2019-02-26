using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections;
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
    public class TestDbParameterCollection : DbParameterCollection
    {
        public List<DbParameter> _Parameters { get; set; } = new List<DbParameter>();
        public override int Count => _Parameters.Count;

        public virtual object _syncRoot { get; set; }
        public override object SyncRoot => _syncRoot;

        public override int Add(object value)
        {
            _Parameters.Add(value as DbParameter);
            return _Parameters.IndexOf(value as DbParameter);
        }

        public override void AddRange(Array values)
        {
            _Parameters.AddRange(values as DbParameter[]);
        }

        public override void Clear()
        {
            _Parameters.Clear();
        }

        public override bool Contains(object value)
        {
            return _Parameters.Any(p => p.Value == value);
        }

        public override bool Contains(string value)
        {
            return _Parameters.Any(p => p.ParameterName == value);
        }

        public override void CopyTo(Array array, int index)
        {
            array.CopyTo(_Parameters.ToArray(), index);
        }

        public override IEnumerator GetEnumerator()
        {
            return _Parameters.GetEnumerator();
        }

        public override int IndexOf(object value)
        {
            return _Parameters.FindIndex(c => c.Value == value);
        }

        public override int IndexOf(string parameterName)
        {
            return _Parameters.FindIndex(c => c.ParameterName == parameterName);
        }

        public override void Insert(int index, object value)
        {
            _Parameters.Insert(index, value as DbParameter);
        }

        public override void Remove(object value)
        {
            _Parameters.Remove(value as DbParameter);
        }

        public override void RemoveAt(int index)
        {
            _Parameters.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            _Parameters.RemoveAt(_Parameters.FindIndex(p => p.ParameterName == parameterName));
        }

        protected override DbParameter GetParameter(int index)
        {
            return _Parameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _Parameters.FirstOrDefault(p => p.ParameterName == parameterName);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _Parameters[index] = value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            RemoveAt(parameterName);
            Add(value);
        }
    }
    public class TestDbCommand : DbCommand
    {
        public TestDbCommand()
        {
            
        }

        public virtual string _CommandText { get; set; }
        public override string CommandText { get => _CommandText; set => _CommandText = value; }

        public virtual int _CommandTimeout { get; set; }
        public override int CommandTimeout { get => _CommandTimeout; set => _CommandTimeout = value; }

        public virtual CommandType _CommandType { get; set; }
        public override CommandType CommandType { get => _CommandType; set => _CommandType = value; }

        public virtual bool _DesignTimeVisible { get; set; }
        public override bool DesignTimeVisible { get => _DesignTimeVisible; set => _DesignTimeVisible = value; }

        public virtual UpdateRowSource _UpdateRowSource { get; set; }
        public override UpdateRowSource UpdatedRowSource { get => _UpdateRowSource; set => _UpdateRowSource = value; }

        public virtual DbConnection _DbConnection { get; set; }
        protected override DbConnection DbConnection { get => _DbConnection; set => _DbConnection = value; }

        public virtual TestDbParameterCollection _DbParameterCollection { get; set; } = new TestDbParameterCollection();
        protected override DbParameterCollection DbParameterCollection => _DbParameterCollection;

        public virtual DbTransaction _DbTransaction { get; set; }
        protected override DbTransaction DbTransaction { get => _DbTransaction; set => _DbTransaction = value; }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public virtual DbParameter _CreateDbParameter() => throw new NotImplementedException();
        protected override DbParameter CreateDbParameter() => _CreateDbParameter();

        public virtual DbDataReader _ExecuteDbDataReader(CommandBehavior behavior) => throw new NotImplementedException();
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => _ExecuteDbDataReader(behavior);

        public virtual Task<DbDataReader> _ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) => throw new NotImplementedException();
        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
            => _ExecuteDbDataReaderAsync(behavior, cancellationToken);
    }

    public class TestDbConnection : DbConnection
    {
        public virtual string _ConnectionString { get; set; }
        public override string ConnectionString { get => _ConnectionString; set => _ConnectionString = value; }

        public virtual string _Database { get; set; }
        public override string Database => _Database;

        public virtual string _DataSource { get; set; }
        public override string DataSource => _DataSource;

        public virtual string _ServerVersion { get; set; }
        public override string ServerVersion => _ServerVersion;

        public ConnectionState _State;
        public override ConnectionState State => _State;

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        public virtual DbTransaction _BeginDbTransaccion(IsolationLevel isolationLevel) => throw new NotImplementedException();
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => _BeginDbTransaccion(isolationLevel);

        public virtual DbCommand _CreateDbCommand() => throw new NotImplementedException();
        protected override DbCommand CreateDbCommand() => _CreateDbCommand();
    }

    public class MetadataBuilderTests
    {

        Mock<DbDataReader> readerMoq;
        Mock<TestDbCommand> cmdMoq;
        Mock<TestDbConnection> connectionMoq;
        Mock<ILogger> loggerMoq = new Mock<ILogger>();
        Mock<ServForOracleCache> cacheMoq = new Mock<ServForOracleCache>(new Mock<IMemoryCache>().Object);

        public class TestRoy
        {
            public string Name { get; set; }
        }

        public MetadataBuilderTests()
        {
            var fix = new Fixture();
            readerMoq = new Mock<DbDataReader>();
            cmdMoq = new Mock<TestDbCommand>() { CallBase = true };
            connectionMoq = new Mock<TestDbConnection>() { CallBase = true };
            var x = connectionMoq.Object;
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
            connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);
            connectionMoq.Setup(c => c._CreateDbCommand()).Returns(cmdMoq.Object);


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
            Assert.Single(result.OracleTypeNetMetadata.Properties);
            Assert.Equal(order, result.OracleTypeNetMetadata.Properties.First().Order);
            Assert.Equal(name, result.OracleTypeNetMetadata.Properties.First().Name);

            Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
            Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

            Assert.IsType<MetadataOracleObject<TestRoy>>(metadata);
            Assert.Equal(metadata, result);
        }

        [Theory, CustomAutoData]
        internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb_Object(OracleUdtInfo info, int[] order, string[] name,
     (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) preset)
        {
            connectionMoq.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => connectionMoq.Object._State = ConnectionState.Open);
            connectionMoq.Setup(c => c._CreateDbCommand()).Returns(cmdMoq.Object);


            cmdMoq.Setup(c => c._ExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).ReturnsAsync(readerMoq.Object);
            readerMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            readerMoq.SetupSequence(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .ReturnsAsync(true);
            
            readerMoq.SetupSequence(r => r.GetInt32(0))
                .Returns(order[0])
                .Returns(order[1]);

            readerMoq.SetupSequence(r => r.GetString(1))
                .Returns(name[0])
                .Returns(name[1]);

            readerMoq.Setup(r => r.GetString(4)).Returns("OBJECT");

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

            //Assert.NotNull(result.OracleTypeNetMetadata.Properties);
            //Assert.Single(result.OracleTypeNetMetadata.Properties);
            //Assert.Equal(order, result.OracleTypeNetMetadata.Properties.First().Order);
            //Assert.Equal(name, result.OracleTypeNetMetadata.Properties.First().Name);

            //Assert.NotNull(result.OracleTypeNetMetadata.UDTInfo);
            //Assert.Equal(info, result.OracleTypeNetMetadata.UDTInfo);

            //Assert.IsType<MetadataOracleObject<TestRoy>>(metadata);
            //Assert.Equal(metadata, result);
        }
    }
}
