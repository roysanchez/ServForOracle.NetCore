using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamClrType<T> : ParamClrType, IParam<T>
    {
        
        internal MetadataOracleCommon Metadata { get; private set; }
        public virtual new T Value { get; private set; }

        public ParamClrType(T value, ParameterDirection direction)
            : this(value, direction, new MetadataOracleCommon())
        {
        }

        internal ParamClrType(T value, ParameterDirection direction, MetadataOracleCommon metadata)
            : base(typeof(T), value, direction)
        {
            Metadata = metadata;
            Value = value;
        }

        internal override async Task SetOutputValueAsync(object value)
        {
            SetOutputValue(value);
            await Task.CompletedTask;
        }

        internal override void SetOutputValue(object value)
        {
            Value = (T)Metadata.ConvertOracleParameterToBaseType(typeof(T), value);
            base.Value = Value;
        }

        internal override OracleParameter GetOracleParameter(string name)
        {
            return Metadata.GetOracleParameter(Type, Direction, name, Value);
        }
    }

    public abstract class ParamClrType : Param
    {
        protected ParamClrType(Type type, object value, ParameterDirection direction)
            : base(type, value, direction)
        {
        }

        internal abstract OracleParameter GetOracleParameter(string name);
    }
}