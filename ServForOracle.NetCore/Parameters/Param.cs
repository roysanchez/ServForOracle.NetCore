using ServForOracle.NetCore.Metadata;
using System;
using System.Data;

namespace ServForOracle.NetCore.Parameters
{
    public class Param<T> : Param
    {
        public MetadataOracle Metadata { get; private set; }
        public new T Value { get; private set; }
        public override bool IsOracleType { get => false; }

        public Param(T value, ParameterDirection direction)
            : base(value, direction)
        {
            Metadata = new MetadataOracle();
            Value = value;
        }

        internal override void SetOutputValue(object value)
        {
            Value = (T)Metadata.ConvertOracleParameterToBaseType(typeof(T), value);
        }
    }

    public abstract class Param
    {
        public ParameterDirection Direction { get; private set; }
        public Param(object value, ParameterDirection direction)
        {
            Value = value;
            Direction = direction;
        }
        public virtual Type Type { get; }
        public virtual object Value { get; protected set; }
        public virtual bool IsOracleType { get; }
        internal abstract void SetOutputValue(object value);

    }
}
