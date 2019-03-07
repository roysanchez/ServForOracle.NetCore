using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Wrapper
{
    public class OracleRefCursorWrapper: IOracleRefCursorWrapper
    {
        private readonly OracleRefCursor _refCursor;
        
        public OracleRefCursorWrapper(OracleRefCursor refCursor)
        {
            _refCursor = refCursor ?? throw new ArgumentNullException(nameof(refCursor));
        }

        public IOracleDataReaderWrapper GetDataReader()
        {
            return new OracleDataReaderWrapper(_refCursor.GetDataReader());
        }
    }
}
