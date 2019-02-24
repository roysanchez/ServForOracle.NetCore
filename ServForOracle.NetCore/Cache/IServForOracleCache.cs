using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Cache
{
    public interface IServForOracleCache
    {
        IMemoryCache Cache { get; }
    }
}
