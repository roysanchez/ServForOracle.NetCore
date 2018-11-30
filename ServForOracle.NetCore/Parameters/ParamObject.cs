using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Metadata;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamObject<T> : ParamObject
    {
        internal MetadataOracleObject<T> Metadata { get; private set; }

        public new T Value { get; private set; }

        private string _ParameterName;

        private OracleUDTInfo _UDTInfo;
        internal override OracleUDTInfo UDTInfo => _UDTInfo;
        public override string ParameterName => _ParameterName;

        public ParamObject(T value, ParameterDirection direction)
            : base(typeof(T), value, direction)
        {
            Value = value;
        }

        public ParamObject(T value, ParameterDirection direction, OracleUDTInfo udtInfo)
            : this(value, direction)
        {
            _UDTInfo = udtInfo ?? throw new ArgumentNullException(nameof(udtInfo));

            if (Type.IsCollection() && !_UDTInfo.IsCollectionValid)
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

        internal override async Task LoadObjectMetadataAsync(MetadataBuilder builder)
        {
            Metadata = await builder.GetOrRegisterMetadataOracleObjectAsync<T>(UDTInfo);
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

        internal override async Task SetOutputValueAsync(object value)
        {
            Value = (T)(await Metadata.GetValueFromRefCursorAsync(Type, value as OracleRefCursor));
            base.Value = Value;
        }

        internal override void SetOutputValue(object value)
        {
            Value = (T)Metadata.GetValueFromRefCursor(Type, value as OracleRefCursor);
            base.Value = Value;
        }

        internal override OracleParameter[] GetOracleParameters(int startNumber)
        {
            return Metadata.GetOracleParameters(Value, startNumber);
        }

        internal override (string Constructor, int LastNumber) BuildQueryConstructorString(string name, int startNumber)
        {
            return Metadata.BuildQueryConstructorString(Value, name, startNumber);
        }

        internal override PreparedOutputParameter PrepareOutputParameter(int startNumber)
        {
            var query = Metadata.GetRefCursorQuery(startNumber, ParameterName);
            var oracleParameter = Metadata.GetOracleParameterForRefCursor(startNumber);

            return new PreparedOutputParameter(this, oracleParameter, query);
        }
    }

    public abstract class ParamObject : Param
    {
        protected ParamObject(Type type, object value, ParameterDirection direction)
            : base(type, value, direction)
        {
        }

        internal virtual OracleUDTInfo UDTInfo { get; }
        public virtual string ParameterName { get; }
        public abstract void SetParameterName(string name);
        public abstract string GetDeclareLine();

        internal bool MetadataLoaded = false;
        internal abstract void LoadObjectMetadata(MetadataBuilder builder);
        internal abstract Task LoadObjectMetadataAsync(MetadataBuilder builder);
        internal abstract (string Constructor, int LastNumber) BuildQueryConstructorString(string name, int startNumber);
        internal abstract OracleParameter[] GetOracleParameters(int startNumber);
        internal abstract PreparedOutputParameter PrepareOutputParameter(int startNumber);
    }
}