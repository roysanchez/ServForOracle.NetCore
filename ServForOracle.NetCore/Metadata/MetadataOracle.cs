using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracle
    {
        private const int VARCHAR_MAX_SIZE = 32000;

        public object ConvertOracleParameterToBaseType(Type retType, object oracleParam)
        {
            bool isNullable = (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Nullable<>));

            object value = null;

            if (oracleParam is null)
            {
                return null;
            }

            if (retType == oracleParam.GetType())
            {
                return oracleParam;
            }

            object param = null;

            if (oracleParam is OracleParameter parameter)
            {
                param = parameter.Value;
            }
            else
            {
                param = oracleParam;
            }

            var castError = new InvalidCastException($"Can't cast a null value to {retType.FullName}");

            switch (param)
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
                    if (CheckIfNotNull(date, isNullable, retType.FullName))
                    {
                        value = date.Value;
                    }
                    break;
                case OracleIntervalDS interval when retType == typeof(TimeSpan) || retType == typeof(TimeSpan?):
                    if (CheckIfNotNull(interval, isNullable, retType.FullName))
                    {
                        value = interval.Value;
                    }
                    break;
                case OracleIntervalYM intervalYM when (
                    retType == typeof(long) || retType == typeof(long?) ||
                    retType == typeof(float) || retType == typeof(float?) ||
                    retType == typeof(double) || retType == typeof(double?)
                ):
                    if (CheckIfNotNull(intervalYM, isNullable, retType.FullName))
                    {
                        value = intervalYM.Value;
                    }
                    break;
                case OracleBinary binary when retType == typeof(byte[]):
                    value = binary.Value;
                    break;
                case OracleTimeStamp timestamp when retType == typeof(DateTime) || retType == typeof(DateTime?):
                    if (CheckIfNotNull(timestamp, isNullable, retType.FullName))
                        value = timestamp;
                    else
                        throw castError;
                    break;
                case OracleTimeStampLTZ timestampLTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                    if (CheckIfNotNull(timestampLTZ, isNullable, retType.FullName))
                        value = timestampLTZ;
                    else
                        throw castError;
                    break;
                case OracleTimeStampTZ timestampTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                    if (CheckIfNotNull(timestampTZ, isNullable, retType.FullName))
                        value = timestampTZ.Value;
                    else
                        throw castError;
                    break;
                default:
                    //TODO Log errors
                    break;
            }

            return value;
        }

        private dynamic GetObjectArrayFromXML(Type listType, XElement xml)
        {
            if (xml is null)
            {
                return null;
            }

            var underType = listType.GetCollectionUnderType();
            dynamic list = listType.CreateInstance();

            foreach (var el in xml.Elements())
            {
                dynamic obj = underType.CreateInstance();

                if (underType.IsCollection())
                {
                    obj = GetObjectArrayFromXML(underType, el);
                }
                else
                {
                    obj = GetObjectFromXML(underType, el);
                }

                if (obj != null)
                {
                    list.Add(obj);
                }
            }

            if (!Enumerable.Any(list))
            {
                return null;
            }
            else if (listType.IsArray)
            {
                return Enumerable.ToArray(list);
            }
            else
            {
                return list;
            }
        }

        private dynamic GetObjectFromXML(Type objectType, XElement xml)
        {
            if (xml is null)
            {
                return null;
            }

            if (objectType.IsClrType())
            {
                return xml.Value;
            }

            dynamic result = objectType.CreateInstance();

            var elements = xml.Elements();
            var nullCounter = 0;

            var properties = objectType.GetProperties();
            foreach (var prop in properties)
            {
                var element = elements.FirstOrDefault(c => c.Name == prop.Name);

                if (element is null || string.IsNullOrWhiteSpace(element.Value))
                {
                    //TODO check if the result can be null
                    prop.SetValue(result, null);
                }
                else if (prop.PropertyType.IsCollection())
                {
                    prop.SetValue(result, GetObjectArrayFromXML(prop.PropertyType, element));
                }
                else if (!prop.PropertyType.IsClrType())
                {
                    prop.SetValue(result, GetObjectFromXML(prop.PropertyType, element));
                }
                else
                {
                    prop.SetValue(result, ConvertXElementToType(prop.PropertyType, element));
                }

                if (prop.GetValue(result) is null)
                    nullCounter++;
            }

            if (nullCounter == properties.Length)
            {
                return null;
            }

            return result;
        }

        private dynamic ConvertXElementToType(Type type, XElement element)
        {
            if (type == typeof(string))
                return (string)element;
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
                return (DateTime?)element;
            else if (type == typeof(int) || type == typeof(int?))
                return (int?)element;
            else if (type == typeof(float) || type == typeof(float?))
                return (float?)element;
            else if (type == typeof(double) || type == typeof(double?))
                return (double?)element;
            else if (type == typeof(decimal) || type == typeof(decimal?))
                return (decimal?)element;
            else if (type == typeof(long) || type == typeof(long?))
                return (long?)element;
            else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
                return (DateTimeOffset?)element;
            else if (type == typeof(bool) || type == typeof(bool?))
                return (bool?)element;
            else if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
                return (TimeSpan?)element;
            else if (type == typeof(uint) || type == typeof(uint?))
                return (uint?)element;
            else if (type == typeof(ulong) || type == typeof(ulong?))
                return (ulong?)element;
            else if (type == typeof(Guid) || type == typeof(Guid?))
                return (Guid?)element;
            else
                return null;
        }

        public dynamic GetValueFromOracleXML(Type retType, OracleXmlType xml)
        {
            if (xml is null || xml.IsNull || retType is null)
            {
                return null;
            }

            XElement doc = XElement.Parse("<roy>" + xml.Value + "</roy>");

            if (retType.IsCollection())
            {
                var list = GetObjectArrayFromXML(retType, doc.Elements().First());
                return list;
            }
            else
            {
                dynamic obj = GetObjectFromXML(retType, doc.Elements().First());
                return obj;
            }
        }

        private bool CheckIfNotNull(INullable value, bool destinationTypeIsNullable, string typeName)
        {
            var castError = new InvalidCastException($"Can't cast a null value to {typeName}"); ;

            if (value.IsNull)
            {
                if (!destinationTypeIsNullable)
                {
                    throw castError;
                }

                return false;
            }
            else
            {
                return true;
            }
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