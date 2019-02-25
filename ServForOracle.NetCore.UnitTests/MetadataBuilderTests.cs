using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class TestDbCommand : DbCommand
    {
        Mock<DbDataReader> _reader;

        public TestDbCommand(Mock<DbDataReader> reader)
        {
            _reader = reader;
        }

        public override string CommandText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool DesignTimeVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        protected override DbConnection DbConnection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();

        protected override DbTransaction DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return _reader.Object;
        }
    }

    public class TestDbConnection : DbConnection
    {
        private readonly Mock<TestDbCommand> _mock;

        public TestDbConnection(Mock<TestDbCommand> mock)
        {
            _mock = mock;
        }

        public override string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string Database => throw new NotImplementedException();

        public override string DataSource => throw new NotImplementedException();

        public override string ServerVersion => throw new NotImplementedException();

        public override ConnectionState State => throw new NotImplementedException();

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

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            return _mock.Object;
        }
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
            cmdMoq = new Mock<TestDbCommand>(readerMoq);
            connectionMoq = new Mock<TestDbConnection>(cmdMoq);
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
        internal async Task GetMetadataOracleObjectAsync_NotInCache_CallsDb(OracleUdtInfo info, MetadataOracleTypeDefinition typeDefinition, UdtPropertyNetPropertyMap[] properties, int order, string name)
        {
            var metadata = new MetadataOracleObject<TestRoy>(cacheMoq.Object, typeDefinition, properties, fuzzyNameMatch: true);

            
            

            //connectionMoq.Setup(c => c.CreateCommand()).Returns(cmd.Object);
            //cmdMoq.Setup(c => c.ExecuteReaderAsync()).ReturnsAsync(readerMoq.Object);
            readerMoq.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
            readerMoq.Setup(r => r.IsDBNullAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            readerMoq.Setup(r => r.GetInt32(0)).Returns(order);
            readerMoq.Setup(r => r.GetString(1)).Returns(name);

            cacheMoq.Setup(c => c.SaveTypeDefinition(It.IsAny<MetadataOracleTypeDefinition>()))
                .Verifiable();

            var builder = new MetadataBuilder(connectionMoq.Object, cacheMoq.Object, loggerMoq.Object);

            var result = await builder.GetOrRegisterMetadataOracleObjectAsync<TestRoy>(info);

            cacheMoq.Verify();
            Assert.Equal(metadata, result);
        }
    }
}
