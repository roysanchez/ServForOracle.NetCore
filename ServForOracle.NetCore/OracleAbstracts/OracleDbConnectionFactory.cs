using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ServForOracle.NetCore.OracleAbstracts
{
    public class OracleDbConnectionFactory: IDbConnectionFactory
    {
        private readonly string _ConnectionString;
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
            return new OracleConnection(_ConnectionString);
        }
    }
}
