using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;

namespace ServForOracle.NetCore.OracleAbstracts
{
    public class OracleDbConnectionFactory: IDbConnectionFactory, IDisposable
    {
        private readonly string _ConnectionString;
        private OracleConnection _OracleConnection;
        public OracleDbConnectionFactory(string connectionString)
        {
            if(string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _ConnectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            _OracleConnection = new OracleConnection(_ConnectionString);

            return _OracleConnection;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _OracleConnection != null)
            {
                _OracleConnection?.Dispose();

                if (_OracleConnection.State == ConnectionState.Open)
                    _OracleConnection.Close();
            }
        }

        ~OracleDbConnectionFactory()
        {
            Dispose(false);
        }

        #endregion
    }
}
