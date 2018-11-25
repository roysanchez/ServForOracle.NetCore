using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Metadata;
using System;
using System.Data;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamObject<T> : ParamObject
    {
        public MetadataOracleObject<T> Metadata { get; private set; }

        public new T Value { get; private set; }

        private readonly string _Schema;
        private readonly string _ObjectName;
        private readonly string _ListName;
        private string _ParameterName;

        public override bool IsOracleType => true;
        public override string Schema => _Schema;
        public override string ObjectName => base.ObjectName;
        public override Type Type => typeof(T);
        public override string ParameterName => _ParameterName;

        public ParamObject(T value, string schema, string objectName, OracleConnection con,
            ParameterDirection direction, string listName = null)
            : base(value, direction)
        {
            _Schema = schema;
            _ObjectName = objectName;
            _ListName = listName;
            Metadata = new MetadataOracleObject<T>(schema, objectName, con);
            Value = value;
        }

        public override void SetParameterName(string name)
        {
            _ParameterName = name;
        }

        public override string GetDeclareLine()
        {
            if (Type.IsCollection())
                return $"{_ParameterName} {_Schema}.{_ListName} := {_Schema}.{_ListName}();";
            else
                return $"{_ParameterName} {_Schema}.{_ObjectName};";
        }

        internal override void SetOutputValue(object value)
        {
            Value = Metadata.GetValueFromRefCursor(value as OracleRefCursor);
        }
    }

    public abstract class ParamObject : Param
    {
        public ParamObject(object value, ParameterDirection direction)
            : base(value, direction)
        {

        }

        public virtual string ParameterName { get; }
        public virtual string Schema { get; }
        public virtual string ObjectName { get; }
        public abstract void SetParameterName(string name);
        public abstract string GetDeclareLine();
    }
}
