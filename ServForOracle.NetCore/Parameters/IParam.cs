using System;
using System.Data;

namespace ServForOracle.NetCore.Parameters
{
    public interface IParam<out T> : IParam
    {
        T Value { get; }
    }

    public interface IParam
    {
        Type Type { get; }
        ParameterDirection Direction { get; }
    }
}
