using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ServForOracle.NetCore.Extensions;
using System.Threading.Tasks;
using System.Data;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleObject<T> : MetadataOracleObject
    {
        private readonly Regex regex = new Regex(Regex.Escape("$"));
        private readonly string ConstructorString;
        private readonly Type Type;
        internal readonly MetadataOracleNetTypeDefinition OracleTypeNetMetadata;

        public MetadataOracleObject(MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch)
        {
            Type = typeof(T);
            if (Type.IsCollection())
            {
                OracleTypeNetMetadata = new MetadataOracleNetTypeDefinition(Type.GetCollectionUnderType(), metadataOracleType, customProperties, fuzzyNameMatch);
            }
            else
            {
                OracleTypeNetMetadata = new MetadataOracleNetTypeDefinition(Type, metadataOracleType, customProperties, fuzzyNameMatch);
            }

            ConstructorString = GenerateConstructor(metadataOracleType.UDTInfo.FullObjectName, metadataOracleType.Properties.ToArray());
        }

        private string GenerateConstructor(string objectName, MetadataOracleTypePropertyDefinition[] properties)
        {
            var constructor = new StringBuilder($"{objectName}(");

            var last = properties.Select(c => c.Order).DefaultIfEmpty().Max();
            foreach (var prop in properties.OrderBy(c => c.Order))
            {
                constructor.Append(prop.Name);
                constructor.Append("=>$");

                if (prop.Order < last)
                {
                    constructor.Append(',');
                }
            }

            constructor.Append(");");
            return constructor.ToString();
        }

        private string BuildConstructor(StringBuilder baseString, object value, string parameterName, MetadataOracleNetTypeDefinition metadata, ref int startNumber, string constructor)
        {
            var workedTypes = new Dictionary<string, string>();
            int dependenciesCounter = 0;
            foreach (var prop in metadata.Properties.Where(c => c.PropertyMetadata != null).OrderBy(c => c.Order))
            {
                var workedName = parameterName + "_" + dependenciesCounter++;
                var subConstructor = GenerateConstructor(prop.PropertyMetadata.UDTInfo.FullObjectName, prop.PropertyMetadata.Properties.ToArray());
                BuildQueryConstructor(baseString, prop.NETProperty.PropertyType, prop.NETProperty.GetValue(value), workedName, ref startNumber, prop.PropertyMetadata, subConstructor);
                workedTypes.Add(prop.Name, workedName);
            }

            foreach (var prop in metadata.Properties.OrderBy(c => c.Order))
            {
                if (prop.NETProperty != null)
                {
                    if (prop.PropertyMetadata != null)
                    {
                        workedTypes.TryGetValue(prop.Name, out var subtype);
                        constructor = regex.Replace(constructor, subtype);
                    }
                    else if (value != null && prop.NETProperty.GetValue(value) != null)
                    {
                        constructor = regex.Replace(constructor, $":{startNumber++}", 1);
                    }
                    else
                    {
                        constructor = regex.Replace(constructor, "null", 1);
                    }
                }
                else
                {
                    constructor = regex.Replace(constructor, "null", 1);
                }

            }

            return constructor;
        }
        
        private void BuildQueryConstructor(StringBuilder baseString, Type type, object value, string name, ref int startNumber, MetadataOracleNetTypeDefinition metadata, string constructor)
        {
            if (type.IsCollection())
            {
                if (value != null)
                {
                    foreach (var v in value as IEnumerable)
                    {
                        var baseConstructor = BuildConstructor(baseString, v, name, metadata, ref startNumber, constructor);
                        baseString.AppendLine($"{name}.extend;");
                        baseString.AppendLine($"{name}({name}.last) := {baseConstructor}");
                    }
                }
            }
            else
            {
                var baseConstructor = BuildConstructor(baseString, value, name, metadata, ref startNumber, constructor);
                baseString.AppendLine($"{name} := {baseConstructor}");
            }
        }

        private IEnumerable<OracleParameter> ProcessOracleParameter(object value, MetadataOracleNetTypeDefinition metadata, int startNumber, out int newNumber)
        {
            var propertiesParameters = new List<OracleParameter>();
            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
            {
                if (prop.PropertyMetadata != null)
                {
                    if (prop.NETProperty.PropertyType.IsCollection())
                    {
                        propertiesParameters.AddRange(ProcessCollectionParameters(prop.NETProperty.GetValue(value) as IEnumerable, prop.PropertyMetadata, startNumber, out startNumber));
                    }
                    else
                    {
                        propertiesParameters.AddRange(ProcessOracleParameter(prop.NETProperty.GetValue(value), prop.PropertyMetadata,
                            startNumber, out startNumber));
                    }
                }
                else
                {
                    if (value != null && prop.NETProperty.GetValue(value) != null)
                    {
                        propertiesParameters.Add(
                        GetOracleParameter(
                            type: prop.NETProperty.PropertyType,
                            direction: ParameterDirection.Input,
                            name: $":{startNumber++}",
                            value: prop.NETProperty.GetValue(value)
                        ));
                    }
                }
            }

            newNumber = startNumber;
            return propertiesParameters;
        }

        private IEnumerable<OracleParameter> ProcessCollectionParameters(IEnumerable value, MetadataOracleNetTypeDefinition metadata, int startNumber, out int lastNumber)
        {
            var rowsParameters = new List<OracleParameter>();
            if (value != null)
            {
                foreach (var temp in value)
                {
                    rowsParameters.AddRange(ProcessOracleParameter(temp, metadata, startNumber, out startNumber));
                }
            }
            lastNumber = startNumber;
            return rowsParameters;
        }

        private string QueryBuilder(MetadataOracleNetTypeDefinition metadata, string tableName, int level = 0, bool isRoot = false)
        {
            var select = new StringBuilder();
            var first = true;

            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    select.AppendLine(",");
                }
                var newTable = $"{tableName}.{prop.Name}";

                if (prop.PropertyMetadata != null)
                {
                    if (prop.NETProperty.PropertyType.IsCollection())
                    {
                        var futureLevel = level + 1;
                        var levelStr = $"d{futureLevel}";
                        select.Append($"(select xmlelement( \"{prop.NETProperty.Name}\",");
                        select.Append($" xmlagg( xmlconcat( xmlelement( \"{prop.NETProperty.Name}\", ");
                        select.Append(QueryBuilder(prop.PropertyMetadata, levelStr, futureLevel));
                        select.Append($") ) ) ) from table({tableName}.{prop.Name}) {levelStr})");

                        if (isRoot)
                        {
                            select.Append($" {prop.Name}");
                        }
                    }
                    else
                    {
                        select.Append($"(select xmlelement( \"{prop.NETProperty.Name}\", xmlconcat( ");
                        select.Append(QueryBuilder(prop.PropertyMetadata, newTable));
                        select.Append(") ) from dual)");

                        if (isRoot)
                        {
                            select.Append($" {prop.NETProperty.Name}");
                        }
                    }
                }
                else
                {
                    if(isRoot)
                    {
                        select.Append($"{tableName}.{prop.Name} {prop.NETProperty.Name}");
                    }
                    else
                    {
                        select.Append($"XmlElement(\"{prop.NETProperty.Name}\", {tableName}.{prop.Name})");
                    }
                }
            }

            if (first)
            {
                //TODO Log error message because it couldn't process any properties
                select.Append("1 dummy");
            }

            return select.ToString();
        }

        private string GetRefCursorCollectionQuery(int startNumber, string fieldName, MetadataOracleNetTypeDefinition oracleTypeNetMetadata)
        {
            var query = new StringBuilder($"open :{startNumber} for select ");

            query.AppendLine(QueryBuilder(oracleTypeNetMetadata, "value(c)", isRoot: true));
            query.Append($" from table({fieldName}) c;");

            return query.ToString();
        }

        private string GetRefCursorObjectQuery(int startNumber, string fieldName, MetadataOracleNetTypeDefinition oracleTypeNetMetadata)
        {
            var query = new StringBuilder($"open :{startNumber} for select ");
            query.AppendLine(QueryBuilder(oracleTypeNetMetadata, $"value({fieldName})", isRoot: true));
            query.Append(" from dual;");

            return query.ToString();
        }

        private dynamic ReadObjectInstance(Type type, OracleDataReader reader, MetadataOracleNetTypeDefinition metadata, ref int count)
        {
            var instance = type.CreateInstance();
            int nullCounter = 0;

            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
            {
                var subType = prop.NETProperty.PropertyType;
                var subInstance = subType.CreateInstance();
                var oracleValue = reader.GetOracleValue(count++);

                if (prop.PropertyMetadata != null)
                {
                    var name = prop.NETProperty.Name;
                    subInstance = GetValueFromOracleXML(subType, oracleValue as OracleXmlType, name);
                }
                else
                {
                    subInstance = ConvertOracleParameterToBaseType(subType, oracleValue);
                }

                prop.NETProperty.SetValue(instance, subInstance);

                if (subInstance is null)
                {
                    nullCounter++;
                }
            }

            return instance;
        }

        private string GetDeclareLine(Type type, string parameterName, OracleUdtInfo udtInfo, MetadataOracleNetTypeDefinition metadata)
        {
            var dependenciesCounter = 0;
            var declareLine = new StringBuilder();
            foreach (var prop in metadata.Properties.Where(c => c.PropertyMetadata != null).OrderBy(c => c.Order))
            {
                var subName = parameterName + "_" + dependenciesCounter++;
                declareLine.Append(GetDeclareLine(prop.NETProperty.PropertyType, subName, prop.PropertyMetadata.UDTInfo, prop.PropertyMetadata));
            }

            if (type.IsCollection())
                declareLine.AppendLine($"{parameterName} {udtInfo.FullCollectionName} := {udtInfo.FullCollectionName}();");
            else
                declareLine.AppendLine($"{parameterName} {udtInfo.FullObjectName};");

            return declareLine.ToString();
        }

        public (string Constructor, int LastNumber) BuildQueryConstructorString(T value, string name, int startNumber)
        {
            var baseString = new StringBuilder();
            BuildQueryConstructor(baseString, Type, value, name, ref startNumber, OracleTypeNetMetadata, ConstructorString);

            return (baseString.ToString(), startNumber);
        }

        public OracleParameter[] GetOracleParameters(T value, int startNumber)
        {
            var parameters = new List<OracleParameter>();
            if (value != null)
            {
                if (Type.IsCollection() && value is IEnumerable list)
                {
                    parameters.AddRange(ProcessCollectionParameters(list, OracleTypeNetMetadata, startNumber, out int _));
                }
                else
                {
                    parameters.AddRange(ProcessOracleParameter(value, OracleTypeNetMetadata, startNumber, out int _));
                }
            }

            return parameters.ToArray();
        }

        public override string GetRefCursorQuery(int startNumber, string fieldName)
        {
            if (Type.IsCollection())
            {
                return GetRefCursorCollectionQuery(startNumber, fieldName, OracleTypeNetMetadata);
            }
            else
            {
                return GetRefCursorObjectQuery(startNumber, fieldName, OracleTypeNetMetadata);
            }
        }

        public override async Task<object> GetValueFromRefCursorAsync(Type type, OracleRefCursor refCursor)
        {
            dynamic instance = type.CreateInstance();
            int counter = 0;
            var reader = refCursor.GetDataReader();

            if (type.IsCollection())
            {
                var subType = type.GetCollectionUnderType();
                while (await reader.ReadAsync())
                {
                    counter = 0;
                    instance.Add(ReadObjectInstance(subType, reader, OracleTypeNetMetadata, ref counter));
                }

                return type.IsArray ? Enumerable.ToArray(instance) : Enumerable.AsEnumerable(instance);
            }
            else
            {
                while (await reader.ReadAsync())
                {
                    ReadObjectInstance(type, reader, OracleTypeNetMetadata, ref counter);
                }

                return (T)instance;
            }
        }

        public override object GetValueFromRefCursor(Type type, OracleRefCursor refCursor)
        {
            dynamic instance = type.CreateInstance();
            int counter = 0;
            var reader = refCursor.GetDataReader();

            if (type.IsCollection())
            {
                var subType = type.GetCollectionUnderType();
                while (reader.Read())
                {
                    counter = 0;
                    instance.Add(ReadObjectInstance(subType, reader, OracleTypeNetMetadata, ref counter));
                }

                return type.IsArray ? Enumerable.ToArray(instance) : Enumerable.AsEnumerable(instance);
            }
            else
            {
                while (reader.Read())
                {
                    instance = ReadObjectInstance(type, reader, OracleTypeNetMetadata, ref counter);
                }

                return (T)instance;
            }
        }

        public string GetDeclareLine(Type type, string parameterName, OracleUdtInfo udtInfo)
        {
            return GetDeclareLine(type, parameterName, udtInfo, OracleTypeNetMetadata);
        }
    }

    internal abstract class MetadataOracleObject : MetadataOracle
    {
        public abstract Task<object> GetValueFromRefCursorAsync(Type type, OracleRefCursor refCursor);
        public abstract object GetValueFromRefCursor(Type type, OracleRefCursor refCursor);
        public abstract string GetRefCursorQuery(int startNumber, string fieldName);
    }
}
