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
        internal override string ParameterName => _ParameterName;
        private string _ParameterName;

        public ParamBoolean(bool? value, ParameterDirection direction)
            : this(value, direction, new MetadataOracleBoolean())
        {
        }

        internal ParamBoolean(bool? value, ParameterDirection direction, MetadataOracleBoolean metadata)
            : base(typeof(bool?), value, direction)
        {
            Value = value;
            Metadata = metadata;
        }

        internal virtual string GetBodyVariableSetString()
        {
            if (Value.HasValue)
            {
                if (Value.Value)
                {
                    return $"{_ParameterName} := true;";
                }
                else
                {
                    return $"{_ParameterName} := false;";
                }
            }
            else
            {
                return $"{_ParameterName} := null;";
            }
        }

        internal override string GetDeclareLine()
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

        internal override void SetParameterName(string name)
        {
            _ParameterName = name;
        }

        internal virtual OracleParameter GetOracleParameter(int startNumber)
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
