using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracle
    {
        public object ConvertOracleParameterToBaseType(Type retType, object oracleParam)
        {
            bool isNullable = (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Nullable<>));

            object value = null;

            if (retType == oracleParam.GetType())
            {
                return oracleParam;
            }

            var castError = new InvalidCastException($"Can't cast a null value to {retType.FullName}");

            switch (oracleParam)
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
                    if(isNullable || !interval.IsNull)
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

            return value;
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
