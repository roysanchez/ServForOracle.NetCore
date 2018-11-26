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

        private string _ParameterName;

        private OracleUDTInfo _UDTInfo;
        internal override OracleUDTInfo UDTInfo => _UDTInfo;
        public override bool IsOracleType => true;
        public override Type Type => typeof(T);
        public override string ParameterName => _ParameterName;

        public ParamObject(T value, ParameterDirection direction)
            : base(value, direction)
        {
            Value = value;
        }

        public ParamObject(T value, ParameterDirection direction, OracleUDTInfo udtInfo)
            : this(value, direction)
        {
            _UDTInfo = udtInfo ?? throw new ArgumentNullException(nameof(udtInfo));

            if(Type.IsCollection() && !_UDTInfo.IsCollectionValid)
            {
                throw new ArgumentException($"For the type {Type.FullName} array you must especify the UDT collection name",
                    nameof(udtInfo));
            }
        }

        internal override void LoadObjectMetadata(MetadataBuilder builder)
        {
            Metadata = builder.GetOrRegisterMetadataOracleObject<T>(UDTInfo);
            _UDTInfo = Metadata.OracleTypeNetMetadata.UDTInfo;
            MetadataLoaded = true;
        }

        public override void SetParameterName(string name)
        {
            _ParameterName = name;
        }

        public override string GetDeclareLine()
        {
            return Metadata.GetDeclareLine(Type, _ParameterName, UDTInfo);
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

        internal virtual OracleUDTInfo UDTInfo { get; }
        public virtual string ParameterName { get; }
        public abstract void SetParameterName(string name);
        public abstract string GetDeclareLine();

        internal bool MetadataLoaded = false;
        internal abstract void LoadObjectMetadata(MetadataBuilder builder);
    }
}