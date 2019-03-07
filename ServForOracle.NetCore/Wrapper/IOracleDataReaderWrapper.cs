using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Wrapper
{
    public interface IOracleDataReaderWrapper
    {
        bool Read();
        Task<bool> ReadAsync();
        object GetOracleValue(int i);
    }
}
