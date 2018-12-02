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
        internal MetadataOracle Metadata { get; private set; }

        bool IParam<bool>.Value => Value ?? false;
        private string Name;

        public ParamBoolean(bool? value, ParameterDirection direction)
            : this(value, direction, new MetadataOracle())
        {
            Value = value;
        }

        internal ParamBoolean(bool? value, ParameterDirection direction, MetadataOracle metadata)
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
                return "";
            }
        }
        internal override void SetOutputValue(object value)
        {
            if (value is OracleDecimal tempValue)
            {
                if (tempValue.Value == 0)
                {
                    Value = false;
                }
                else if (tempValue.Value == 1)
                {
                    Value = true;
                }
            }
        }

        internal override Task SetOutputValueAsync(object value)
        {
            SetOutputValue(value);
            return Task.CompletedTask;
        }

        public override void SetParameterName(string name)
        {
            Name = name;
        }

        internal OracleParameter GetOracleParameters(int startNumber)
        {
            if (Direction == ParameterDirection.Output || Direction == ParameterDirection.InputOutput)
            {
                byte? tempValue = null;
                if (Value.HasValue)
                {
                    tempValue = Value.Value ? (byte)1 : (byte)0;
                }

                return new OracleParameter()
                {
                    ParameterName = Name,
                    Direction = Direction,
                    Value = tempValue
                };
            }
            else
            {
                return new OracleParameter(Name, Value);
            }
        }

        internal override PreparedOutputParameter PrepareOutputParameter(int startNumber)
        {
            return new PreparedOutputParameter(this, GetOracleParameters(startNumber), "roy");
        }
    }
}
