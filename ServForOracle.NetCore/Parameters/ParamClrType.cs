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
        private const int VARCHAR_MAX_SIZE = 32000;
        internal MetadataOracle Metadata { get; private set; }
        public new T Value { get; private set; }

        public ParamClrType(T value, ParameterDirection direction)
            : this(value, direction, new MetadataOracle())
        {
        }

        internal ParamClrType(T value, ParameterDirection direction, MetadataOracle metadata)
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
            var param = new OracleParameter(name, Value)
            {
                Direction = Direction
            };

            if (Type.IsValueType)
            {
                if (Type == typeof(char) || Type == typeof(char?))
                {
                    param.OracleDbType = OracleDbType.Char;
                }
                else if (Type == typeof(sbyte) || Type == typeof(sbyte?))
                {
                    param.OracleDbType = OracleDbType.Byte;
                }
                else if (Type == typeof(short) || Type == typeof(short?)
                    || Type == typeof(byte) || Type == typeof(byte?))
                {
                    param.OracleDbType = OracleDbType.Int16;
                }
                else if (Type == typeof(int) || Type == typeof(int?))
                {
                    param.OracleDbType = OracleDbType.Int32;
                }
                else if (Type == typeof(long) || Type == typeof(long?))
                {
                    param.OracleDbType = OracleDbType.Int64;
                }
                else if (Type == typeof(float) || Type == typeof(float?))
                {
                    param.OracleDbType = OracleDbType.Single;
                }
                else if (Type == typeof(double) || Type == typeof(double?))
                {
                    param.OracleDbType = OracleDbType.Double;
                }
                else if (Type == typeof(decimal) || Type == typeof(decimal?))
                {
                    param.OracleDbType = OracleDbType.Decimal;
                }
                else if (Type == typeof(DateTime) || Type == typeof(DateTime?))
                {
                    param.OracleDbType = OracleDbType.Date;
                }
                else if (Type == typeof(bool) || Type == typeof(bool?))
                {
                    param.OracleDbType = OracleDbType.Boolean;
                }
                // TODO Log Error
            }
            else if (Type.IsArray && Type == typeof(byte[]))
            {
                param.OracleDbType = OracleDbType.Blob;
            }
            else if (Type == typeof(string))
            {
                param.OracleDbType = OracleDbType.Varchar2;
                if (Direction != ParameterDirection.Input)
                    param.Size = VARCHAR_MAX_SIZE;

                if (Value != null && Value is string str && str.Length > VARCHAR_MAX_SIZE)
                {
                    param.OracleDbType = OracleDbType.Clob;
                    param.Size = default;
                }
            }
            //TODO Log Error

            return param;
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