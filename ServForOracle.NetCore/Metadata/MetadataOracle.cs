using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracle
    {
        private const int VARCHAR_MAX_SIZE = 32000;

        public object ConvertOracleParameterToBaseType(Type retType, object oracleParam)
        {
            bool isNullable = (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Nullable<>));

            object value = null;

            if(oracleParam is null)
            {
                return null;
            }

            if (retType == oracleParam.GetType())
            {
                return oracleParam;
            }

            var castError = new InvalidCastException($"Can't cast a null value to {retType.FullName}");

            if(oracleParam is OracleParameter param)
            {
                switch (param.Value)
                {
                    case DBNull nulo:
                        break;
                    case OracleDecimal dec:
                        if (dec.IsNull)
                        {
                            if (isNullable || !retType.IsValueType || retType == typeof(string))
                                value = null;
                            else
                                throw castError;
                        }
                        else if (retType == typeof(int) || retType == typeof(int?))
                            value = dec.ToInt32();
                        else if (retType == typeof(float) || retType == typeof(float?))
                            value = dec.ToSingle();
                        else if (retType == typeof(double) || retType == typeof(double?))
                            value = dec.ToDouble();
                        else if (retType == typeof(decimal) || retType == typeof(decimal?))
                            value = dec.Value;
                        else if (retType == typeof(byte) || retType == typeof(byte?))
                            value = dec.ToByte();
                        else if (retType == typeof(string))
                            value = dec.ToString();
                        else
                            throw new InvalidCastException($"Can't cast an OracleDecimal to {retType.FullName}, received val:"
                                + dec.ToString());
                        break;
                    case OracleString str when retType == typeof(string):
                        if (str.IsNull)
                            value = null;
                        else
                            value = str.ToString();
                        break;
                    case OracleClob clob when retType == typeof(string):
                        value = clob.Value;
                        break;
                    case OracleBFile file when retType == typeof(byte[]):
                        value = file.Value;
                        break;
                    case OracleBlob blob when retType == typeof(byte[]):
                        value = blob.Value;
                        break;
                    case OracleDate date when retType == typeof(DateTime) || retType == typeof(DateTime?):
                        if (isNullable || !date.IsNull)
                            value = date.Value;
                        else
                            throw castError;
                        break;
                    case OracleIntervalDS interval when retType == typeof(TimeSpan) || retType == typeof(TimeSpan?):
                        if (isNullable || !interval.IsNull)
                            value = interval.Value;
                        else
                            throw new InvalidCastException($"Can't cast a null value to {retType.FullName}");
                        break;
                    case OracleIntervalYM intervalYM when (
                        retType == typeof(long) || retType == typeof(long?) ||
                        retType == typeof(float) || retType == typeof(float?) ||
                        retType == typeof(double) || retType == typeof(double?)
                    ):
                        if (isNullable || !intervalYM.IsNull)
                            value = intervalYM.Value;
                        else
                            throw castError;
                        break;
                    case OracleBinary binary when retType == typeof(byte[]):
                        value = binary.Value;
                        break;
                    case OracleTimeStamp timestamp when retType == typeof(DateTime) || retType == typeof(DateTime?):
                        if (isNullable || !timestamp.IsNull)
                            value = timestamp;
                        else
                            throw castError;
                        break;
                    case OracleTimeStampLTZ timestampLTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                        if (isNullable || !timestampLTZ.IsNull)
                            value = timestampLTZ;
                        else
                            throw castError;
                        break;
                    case OracleTimeStampTZ timestampTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                        if (isNullable || !timestampTZ.IsNull)
                            value = timestampTZ;
                        else
                            throw castError;
                        break;
                    default:
                        //Log errors
                        break;
                }
            }

            

            return value;
        }

        public OracleParameter GetOracleParameter(Type type, ParameterDirection direction, string name, object value)
        {
            var param = new OracleParameter(name, value)
            {
                Direction = direction
            };

            if (type.IsValueType)
            {
                if (type == typeof(char) || type == typeof(char?))
                {
                    param.OracleDbType = OracleDbType.Char;
                }
                else if (type == typeof(sbyte) || type == typeof(sbyte?))
                {
                    param.OracleDbType = OracleDbType.Byte;
                }
                else if (type == typeof(short) || type == typeof(short?)
                    || type == typeof(byte) || type == typeof(byte?))
                {
                    param.OracleDbType = OracleDbType.Int16;
                }
                else if (type == typeof(int) || type == typeof(int?))
                {
                    param.OracleDbType = OracleDbType.Int32;
                }
                else if (type == typeof(long) || type == typeof(long?))
                {
                    param.OracleDbType = OracleDbType.Int64;
                }
                else if (type == typeof(float) || type == typeof(float?))
                {
                    param.OracleDbType = OracleDbType.Single;
                }
                else if (type == typeof(double) || type == typeof(double?))
                {
                    param.OracleDbType = OracleDbType.Double;
                }
                else if (type == typeof(decimal) || type == typeof(decimal?))
                {
                    param.OracleDbType = OracleDbType.Decimal;
                }
                else if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    param.OracleDbType = OracleDbType.Date;
                }
                else if (type == typeof(bool) || type == typeof(bool?))
                {
                    param.OracleDbType = OracleDbType.Boolean;
                }
                // TODO Log Error
            }
            else if (type.IsArray && type == typeof(byte[]))
            {
                param.OracleDbType = OracleDbType.Blob;
            }
            else if (type == typeof(string))
            {
                param.OracleDbType = OracleDbType.Varchar2;
                if (direction != ParameterDirection.Input)
                    param.Size = VARCHAR_MAX_SIZE;

                if (value != null && value is string str && str.Length > VARCHAR_MAX_SIZE)
                {
                    param.OracleDbType = OracleDbType.Clob;
                    param.Size = default;
                }
            }
            //TODO Log Error

            return param;
        }


        public OracleParameter GetOracleParameterForRefCursor(int starNumber)
        {
            return new OracleParameter($":{starNumber}", DBNull.Value)
            {
                OracleDbType = OracleDbType.RefCursor
            };
        }
    }
}
