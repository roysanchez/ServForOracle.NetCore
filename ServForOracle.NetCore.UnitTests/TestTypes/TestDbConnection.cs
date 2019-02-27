using System;
using System.Data;
using System.Data.Common;

namespace ServForOracle.NetCore.UnitTests.TestTypes
{
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
}
