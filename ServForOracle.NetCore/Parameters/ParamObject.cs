using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Metadata;
using System;
using System.Data;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamObject<T> : ParamObject
    {
        internal MetadataOracleObject<T> Metadata { get; private set; }

        public new T Value { get; private set; }

        private readonly string _UserParameterSchema;
        private readonly string _UserParameterObjectName;
        private readonly string _UserParameterListName;
        private string _ParameterName;

        public override bool IsOracleType => true;
        public override string ObjectName => string.IsNullOrWhiteSpace(_UserParameterSchema) ? Metadata?.OracleTypeNetMetadata.FullObjectName
            : $"{_UserParameterSchema}.{_UserParameterObjectName}";
        public override string CollectioName =>
            string.IsNullOrWhiteSpace(_UserParameterSchema) ? Metadata?.OracleTypeNetMetadata.FullCollectionName
            : $"{_UserParameterSchema}.{_UserParameterListName}";
        public override Type Type => typeof(T);
        public override string ParameterName => _ParameterName;

        public ParamObject(T value, ParameterDirection direction)
            : base(value, direction)
        {
            Value = value;
        }

        public ParamObject(T value, ParameterDirection direction, string schema, string objectName, string listName = null)
            : this(value, direction)
        {
            _UserParameterSchema = schema;
            _UserParameterObjectName = objectName;
            _UserParameterListName = listName;
        }

        internal override void LoadObjectMetadata(MetadataBuilder builder)
        {
            Metadata = builder.GetOrRegisterMetadataOracleObject<T>(_UserParameterSchema, _UserParameterObjectName, _UserParameterListName);
            MetadataLoaded = true;
        }

        public override void SetParameterName(string name)
        {
            _ParameterName = name;
        }

        public override string GetDeclareLine()
        {
            if (Type.IsCollection())
                return $"{_ParameterName} {CollectioName} := {CollectioName}();";
            else
                return $"{_ParameterName} {ObjectName};";
        }

        internal override void SetOutputValue(object value)
        {
            Value = (T)Metadata.GetValueFromRefCursor(Type, value as OracleRefCursor);
        }
    }

    public abstract class ParamObject : Param
    {
        public ParamObject(object value, ParameterDirection direction)
            : base(value, direction)
        {

        }

        public virtual string ObjectName { get; }
        public virtual string CollectioName { get; }
        public virtual string ParameterName { get; }
        //public virtual string Schema { get; }
        //public virtual string ObjectName { get; }
        public abstract void SetParameterName(string name);
        public abstract string GetDeclareLine();

        internal bool MetadataLoaded = false;
        internal abstract void LoadObjectMetadata(MetadataBuilder builder);
    }
}