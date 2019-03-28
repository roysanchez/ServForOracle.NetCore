using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Wrapper
{
    internal interface IOracleRefCursorWrapperFactory
    {
        OracleRefCursorWrapper Create(OracleRefCursor refCursor);
    }

    internal class OracleRefCursorWrapperFactory : IOracleRefCursorWrapperFactory
    {
        public OracleRefCursorWrapper Create(OracleRefCursor refCursor)
        {
            return new OracleRefCursorWrapper(refCursor);
        }
    }
}
