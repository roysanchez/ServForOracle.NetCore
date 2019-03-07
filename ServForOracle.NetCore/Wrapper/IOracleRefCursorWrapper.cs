using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Wrapper
{
    public interface IOracleRefCursorWrapper
    {
        IOracleDataReaderWrapper GetDataReader();
    }
}
