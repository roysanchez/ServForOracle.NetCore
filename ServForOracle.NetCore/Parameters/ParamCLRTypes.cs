using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServForOracle.NetCore.Parameters
{
    public class ParamCLRType<T> : ParamCLRType
    {
        private const int VARCHAR_MAX_SIZE = 32000;
        internal MetadataOracle Metadata { get; private set; }
        public new T Value { get; private set; }

        public ParamCLRType(T value, ParameterDirection direction)
            : this(value, direction, new MetadataOracle())
        {
        }

        internal ParamCLRType(T value, ParameterDirection direction, MetadataOracle metadata)
            : base(typeof(T), value, direction)
        {
            Metadata = metadata;
            Value = value;
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
                else if (Type == typeof(long) || Type == typeof(long))
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
                //else
                //    throw new Exception(string.Format(TypeNotConfiguredMessage, type.Name));
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
            //else
            //    throw new Exception(string.Format(InvalidClassMessage, type.Name));

            return param;
        }
    }

    public abstract class ParamCLRType: Param
    {
        public ParamCLRType(Type type, object value, ParameterDirection direction)
            :base(type, value, direction)
        {
        }

        internal abstract OracleParameter GetOracleParameter(string name);
    }
}
