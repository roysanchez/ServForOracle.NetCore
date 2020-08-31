using System;
using System.Data.Common;

namespace ServForOracle.NetCore.OracleAbstracts
{
    public interface IDbConnectionFactory: IDisposable
    {
        DbConnection CreateConnection();
    }
}
