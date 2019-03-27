using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.Wrapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamObject<T> : ParamObject, IParam<T>
    {
        internal MetadataOracleObject<T> Metadata { get; private set; }

        public new virtual T Value { get; private set; }

        private string _ParameterName;

        internal override string ParameterName => _ParameterName;

        public ParamObject(T value, ParameterDirection direction)
            : base(typeof(T), value, direction)
        {
            Value = value;
        }

        public ParamObject(T value, ParameterDirection direction, OracleUdtInfo udtInfo)
            : this(value, direction)
        {
            UDTInfo = udtInfo ?? throw new ArgumentNullException(nameof(udtInfo));

            if (Type.IsCollection() && !UDTInfo.IsCollectionValid)
            {
                throw new ArgumentException($"For the type {Type.FullName} array you must especify the UDT collection name",
                    nameof(udtInfo));
            }
        }

        internal override void LoadObjectMetadata(MetadataBuilder builder)
        {
            Metadata = builder.GetOrRegisterMetadataOracleObject<T>(UDTInfo);
            UDTInfo = Metadata.OracleTypeNetMetadata.UDTInfo;
            MetadataLoaded = true;
        }

        internal override async Task LoadObjectMetadataAsync(MetadataBuilder builder)
        {
            Metadata = await builder.GetOrRegisterMetadataOracleObjectAsync<T>(UDTInfo).ConfigureAwait(false);
            UDTInfo = Metadata.OracleTypeNetMetadata.UDTInfo;
            MetadataLoaded = true;
        }

        internal override void SetParameterName(string name)
        {
            _ParameterName = name;
        }

        internal override string GetDeclareLine()
        {
            return Metadata.GetDeclareLine(Type, _ParameterName, UDTInfo);
        }

        internal override async Task SetOutputValueAsync(object value)
        {
            Value = (T)(await Metadata.GetValueFromRefCursorAsync(Type, new OracleRefCursorWrapper(value as OracleRefCursor)).ConfigureAwait(false));
            base.Value = Value;
        }

        internal override void SetOutputValue(object value)
        {
            Value = (T)Metadata.GetValueFromRefCursor(Type, new OracleRefCursorWrapper(value as OracleRefCursor));
            base.Value = Value;
        }

        internal override OracleParameter[] GetOracleParameters(int startNumber)
        {
            return Metadata.GetOracleParameters(Value, startNumber);
        }

        internal override (string Constructor, int LastNumber) BuildQueryConstructorString(int startNumber)
        {
            return Metadata.BuildQueryConstructorString(Value, _ParameterName, startNumber);
        }

        internal override PreparedOutputParameter PrepareOutputParameter(int startNumber)
        {
            var query = Metadata.GetRefCursorQuery(startNumber, ParameterName);
            var oracleParameter = Metadata.GetOracleParameterForRefCursor(startNumber);

            return new PreparedOutputParameter(this, oracleParameter, query);
        }
    }

    public abstract class ParamObject : ParamManaged
    {
        protected ParamObject(Type type, object value, ParameterDirection direction)
            : base(type, value, direction)
        {
        }

        protected internal OracleUdtInfo UDTInfo { get; protected set; }
        internal bool MetadataLoaded = false;
        internal abstract void LoadObjectMetadata(MetadataBuilder builder);
        internal abstract Task LoadObjectMetadataAsync(MetadataBuilder builder);
        internal abstract (string Constructor, int LastNumber) BuildQueryConstructorString(int startNumber);
        internal abstract OracleParameter[] GetOracleParameters(int startNumber);
    }
}