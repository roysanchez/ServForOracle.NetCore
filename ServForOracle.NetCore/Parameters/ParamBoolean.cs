using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Metadata;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamBoolean : ParamManaged, IParam<bool?>, IParam<bool>
    {
        public new bool? Value { get; set; }
        internal MetadataOracleBoolean Metadata { get; private set; }

        bool IParam<bool>.Value => Value ?? false;
        private string _ParameterName;

        public ParamBoolean(bool? value, ParameterDirection direction)
            : this(value, direction, new MetadataOracleBoolean())
        {
            Value = value;
        }

        internal ParamBoolean(bool? value, ParameterDirection direction, MetadataOracleBoolean metadata)
            : base(typeof(bool?), value, direction)
        {
            Metadata = metadata;
        }

        public override string GetDeclareLine()
        {
            if (Direction == ParameterDirection.Input)
            {
                return null;
            }
            else
            {
                return Metadata.GetDeclareLine(_ParameterName);
            }
        }

        internal override void SetOutputValue(object value)
        {
            if (value is OracleDecimal tempValue)
            {
                Value = (bool?)Metadata.GetBooleanValue(tempValue);
                base.Value = Value;
            }
        }

        internal override Task SetOutputValueAsync(object value)
        {
            SetOutputValue(value);
            return Task.CompletedTask;
        }

        public override void SetParameterName(string name)
        {
            _ParameterName = name;
        }

        internal OracleParameter GetOracleParameter(int startNumber)
        {
            if (Direction == ParameterDirection.Output || Direction == ParameterDirection.InputOutput)
            {
                return Metadata.GetOracleParameter(Value, startNumber);
            }
            else
            {
                return new OracleParameter(_ParameterName, Value);
            }
        }

        internal override PreparedOutputParameter PrepareOutputParameter(int startNumber)
        {
            var output = Metadata.OutputString(startNumber, _ParameterName);
            return new PreparedOutputParameter(this, GetOracleParameter(startNumber), output);
        }
    }
}
