using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Wrapper
{
    public class OracleDataReaderWrapper: IOracleDataReaderWrapper
    {
        private readonly OracleDataReader _oracleDataReader;
        public OracleDataReaderWrapper(OracleDataReader oracleDataReader)
        {
            _oracleDataReader = oracleDataReader ?? throw new ArgumentNullException(nameof(oracleDataReader));
        }

        public bool Read()
        {
            return _oracleDataReader.Read();
        }

        public Task<bool> ReadAsync()
        {
            return _oracleDataReader.ReadAsync();
        }

        public object GetOracleValue(int i)
        {
            return _oracleDataReader.GetOracleValue(i);
        }
    }
}
