using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.UnitTests.TestTypes
{
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
}
