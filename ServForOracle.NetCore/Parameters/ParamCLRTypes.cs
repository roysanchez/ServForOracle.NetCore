using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamCLRType<T> : ParamCLRType
    {
        internal MetadataOracle Metadata { get; private set; }
        public new T Value { get; private set; }

        public ParamCLRType(T value, ParameterDirection direction)
            : this(value, direction, new MetadataOracle())
        {
        }

        internal ParamCLRType(T value, ParameterDirection direction, MetadataOracle metadata)
            : base(value, direction)
        {
            Metadata = metadata;
            Value = value;
        }

        internal override void SetOutputValue(object value)
        {
            Value = (T)Metadata.ConvertOracleParameterToBaseType(typeof(T), value);
            base.Value = Value;
        }

        internal override OracleParameter GetOracleParameter(string name)
        {
            //TODO handle outputs quirks
            return new OracleParameter(name, Value)
            {
                Direction = Direction
            };
        }
    }

    public abstract class ParamCLRType: Param
    {
        public ParamCLRType(object value, ParameterDirection direction)
            :base(value, direction)
        {
        }

        internal abstract OracleParameter GetOracleParameter(string name);
    }
}
