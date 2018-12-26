using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ServForOracle.NetCore.OracleAbstracts
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
