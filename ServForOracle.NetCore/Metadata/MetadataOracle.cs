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
        
        
        //public MetadataOracle(Type type, MetadataOracleTypeDefinition oracleTypeMetadata)
        //{
            
        //}

        public object ConvertOracleParameterToBaseType(Type retType, object oracleParam)
        {
            bool isNullable = (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Nullable<>));

            object value = null;

            if (retType == oracleParam.GetType())
            {
                return oracleParam;
            }
            
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
                            throw new InvalidCastException($"Can't cast a null value to {retType.Name}");
                    }
                    else if (retType == typeof(int) || retType == typeof(int?))
                        value = dec.ToInt32();
                    else if (retType == typeof(float) || retType == typeof(float?) || retType == typeof(Single))
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
                        throw new InvalidCastException($"Can't cast OracleDecimal to {retType.Name}");
                    break;
                case OracleString str when retType == typeof(string):
                    if (str.IsNull)
                        value = null;
                    else
                        value = str.ToString();
                    break;
                //case OracleClob clob when retType == typeof(string):
                //    value = ExtractValue(clob);
                //    break;
                //case OracleBFile file when retType == typeof(byte[]):
                //    value = ExtractValue(file);
                //    break;
                //case OracleBlob blob when retType == typeof(byte[]):
                //    value = ExtractValue(blob);
                //    break;
                //case OracleDate date when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    value = ExtractNullableValue(date, isNullable);
                //    break;
                //case OracleIntervalDS interval when retType == typeof(TimeSpan) || retType == typeof(TimeSpan?):
                //    value = ExtractNullableValue(interval, isNullable);
                //    break;
                //case OracleIntervalYM intervalYM when (
                //        retType == typeof(long) || retType == typeof(long?) ||
                //        retType == typeof(float) || retType == typeof(float?) ||
                //        retType == typeof(double) || retType == typeof(double?)
                //    ):
                //    value = ExtractNullableValue(intervalYM, isNullable);
                //    break;
                //case OracleBinary binary when retType == typeof(byte[]):
                //    value = ExtractValue(binary);
                //    break;
                //case OracleRef reff when retType == typeof(string):
                //    value = ExtractValue(reff);
                //    break;
                //case OracleTimeStamp timestamp when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    ExtractNullableValue(timestamp, isNullable);
                //    break;
                //case OracleTimeStampLTZ timestampLTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    ExtractNullableValue(timestampLTZ, isNullable);
                //    break;
                //case OracleTimeStampTZ timestampTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    ExtractNullableValue(timestampTZ, isNullable);
                //    break;
                default:
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
