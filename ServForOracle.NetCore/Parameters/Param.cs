using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using System;
using System.Data;

namespace ServForOracle.NetCore.Parameters
{
    public abstract class Param
    {
        public ParameterDirection Direction { get; private set; }
        protected internal Param(object value, ParameterDirection direction)
        {
            Value = value;
            Direction = direction;
        }
        public virtual Type Type { get; }
        public virtual object Value { get; protected set; }
        internal abstract void SetOutputValue(object value);
    }
}
