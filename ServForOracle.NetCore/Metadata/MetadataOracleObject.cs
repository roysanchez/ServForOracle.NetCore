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
        private readonly Regex regex;
        private readonly string ConstructorString;
        private readonly Type Type;
        internal readonly MetadataOracleNetTypeDefinition OracleTypeNetMetadata;
        internal Dictionary<string, string> ObjectSubTypes { get; set; } = new Dictionary<string, string>();

        public MetadataOracleObject(MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch)
        {
            Type = typeof(T);
            if (Type.IsCollection())
            {
                OracleTypeNetMetadata = new MetadataOracleNetTypeDefinition(Type.GetCollectionUnderType(), metadataOracleType,
                    customProperties ?? new UdtPropertyNetPropertyMap[] { }, fuzzyNameMatch);
            }
            else
            {
                OracleTypeNetMetadata = new MetadataOracleNetTypeDefinition(Type, metadataOracleType,
                    customProperties ?? new UdtPropertyNetPropertyMap[] { }, fuzzyNameMatch);
            }

            regex = new Regex(Regex.Escape("$"));

            //var constructor = new StringBuilder();
            ConstructorString = GenerateConstructor(metadataOracleType.UDTInfo.FullObjectName,
                metadataOracleType.Properties.ToArray());

            //constructor.Append(';');
            //ConstructorString = constructor.ToString();
        }

        private string GenerateConstructor(string objectName, MetadataOracleTypePropertyDefinition[] properties)
        {
            var constructor = new StringBuilder($"{objectName}(");
            //constructor.Append($"{objectName}(");

            var last = properties.Select(c => c.Order).DefaultIfEmpty().Max();
            foreach (var prop in properties.OrderBy(c => c.Order))
            {
                constructor.Append(prop.Name);
                constructor.Append("=>$");
                //if (prop is MetadataOracleTypeSubTypeDefinition subType)
                //{
                //    GenerateConstructor(subType.MetadataOracleType.UDTInfo.FullObjectName, subType.MetadataOracleType.Properties.ToArray());

                //    //ObjectSubTypes.Add(objectName + prop.Name, GenerateConstructor(subType.MetadataOracleType.UDTInfo.FullObjectName, subType.MetadataOracleType.Properties.ToArray()));
                //}

                if (prop.Order < last)
                {
                    constructor.Append(',');
                }
            }

            //for (var counter = 0; counter < properties.Count(); counter++)
            //{
            //    constructor.Append(properties[counter].Name);
            //    constructor.Append("=>$");
            //    if (properties[counter] is MetadataOracleTypeSubTypeDefinition subType)
            //    {
            //        //constructor.Append("$");
            //        ObjectSubTypes.Add(properties[counter].Name, GenerateConstructor(subType.MetadataOracleType.UDTInfo.FullObjectName, subType.MetadataOracleType.Properties.ToArray()));
            //        //GenerateConstructor(constructor, subType.MetadataOracleType.UDTInfo.FullObjectName, subType.MetadataOracleType.Properties.ToArray());
            //    }
            //    else
            //    {
            //        //constructor.Append('$');
            //    }

            //    if (counter + 1 < properties.Count())
            //    {
            //        constructor.Append(',');
            //    }
            //}

            constructor.Append(");");
            return constructor.ToString();
        }

        private string BuildConstructor(StringBuilder baseString, object value, string parameterName, MetadataOracleNetTypeDefinition metadata, ref int startNumber,
            string constructor)
        {
            var workedTypes = new Dictionary<string, string>();
            //var dependencies = new StringBuilder();
            int dependenciesCounter = 0;
            foreach (var prop in metadata.Properties.Where(c => c.PropertyMetadata != null).OrderBy(c => c.Order))
            {
                var workedName = parameterName + "_" + dependenciesCounter++;
                var subConstructor = GenerateConstructor(prop.PropertyMetadata.UDTInfo.FullObjectName,
                    prop.PropertyMetadata.Properties.ToArray());
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

            //dependencies.AppendLine(constructor);
            return constructor;//dependencies.ToString(); ;
        }

        public (string Constructor, int LastNumber) BuildQueryConstructorString(T value, string name, int startNumber)
        {
            var baseString = new StringBuilder();
            BuildQueryConstructor(baseString, Type, value, name, ref startNumber, OracleTypeNetMetadata, ConstructorString);

            return (baseString.ToString(), startNumber);
        }

        private void BuildQueryConstructor(StringBuilder baseString, Type type, object value, string name, ref int startNumber, MetadataOracleNetTypeDefinition metadata, string constructor)
        {
            //var baseString = new StringBuilder();
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
                var baseConstructor = BuildConstructor(baseString, value, name, OracleTypeNetMetadata, ref startNumber, constructor);
                baseString.AppendLine($"{name} := {baseConstructor}");
            }

            //return baseString.ToString();
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

        private IEnumerable<OracleParameter> ProcessOracleParameter(object value,
            MetadataOracleNetTypeDefinition metadata,
            int startNumber, out int newNumber)
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
                    propertiesParameters.Add(
                        GetOracleParameter(
                            type: prop.NETProperty.PropertyType,
                            direction: ParameterDirection.Input,
                            name: $":{startNumber++}",
                            value: value != null ? prop.NETProperty.GetValue(value) : null
                        ));
                }
            }
            newNumber = startNumber;
            return propertiesParameters;
        }

        private IEnumerable<OracleParameter> ProcessCollectionParameters(IEnumerable value,
            MetadataOracleNetTypeDefinition metadata, int startNumber, out int lastNumber)
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


        private string QueryBuilder(MetadataOracleNetTypeDefinition metadata, string tableName, int level = 0)
        {
            var select = new StringBuilder();
            var first = true;

            var futureLevel = $"d{level + 1}";

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
                        var levelStr = $"d{++level}";
                        select.Append($"(select xmlagg( xmlelement(\"{prop.Name}\", xmlforest( ");
                        select.Append(QueryBuilder(prop.PropertyMetadata, levelStr, level));
                        select.Append($") ) ) from table({tableName}.{prop.Name}) {futureLevel}) {prop.Name}");
                    }
                    else
                    {
                        select.Append($"(select xmlelement(\"{prop.Name}\", xmlforest( ");
                        select.Append(QueryBuilder(prop.PropertyMetadata, newTable, 0));
                        select.Append($") ) from dual) {prop.Name}");
                        //select.Append(QueryBuilder(prop.PropertyMetadata, tableName, prop.Name));
                    }
                    //}

                }
                else
                {
                    select.Append($"{tableName}.{prop.Name} {prop.Name}");
                }
            }

            if (first)
            {
                //TODO Log error message
                select.Append("1 dummy");
            }

            return select.ToString();
        }

        private string GetRefCursorCollectionQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber} for select ");

            query.AppendLine(QueryBuilder(OracleTypeNetMetadata, "value(c)"));
            query.Append($" from table({fieldName}) c;");

            return query.ToString();
        }

        private string GetRefCursorObjectQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber} for select ");
            query.AppendLine(QueryBuilder(OracleTypeNetMetadata, $"value({fieldName})"));
            query.Append(" from dual;");

            return query.ToString();
        }

        public override string GetRefCursorQuery(int startNumber, string fieldName)
        {
            if (Type.IsCollection())
            {
                return GetRefCursorCollectionQuery(startNumber, fieldName);
            }
            else
            {
                return GetRefCursorObjectQuery(startNumber, fieldName);
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

        private dynamic ReadObjectInstance(Type type, OracleDataReader reader, MetadataOracleNetTypeDefinition metadata, ref int count)
        {
            var instance = type.CreateInstance();
            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
            {
                if (prop.PropertyMetadata != null)
                {
                    if (prop.NETProperty.PropertyType.IsCollection())
                    {
                        var x = GetObjectArrayFromOracleXML(prop.NETProperty.PropertyType, reader.GetOracleValue(count++) as OracleXmlType, prop.Name);
                        prop.NETProperty.SetValue(instance, x);
                    }
                    else
                    {
                        prop.NETProperty.SetValue(instance, ReadObjectInstance(prop.NETProperty.PropertyType,
                            reader, prop.PropertyMetadata, ref count));
                    }
                }
                else
                {
                    prop.NETProperty.SetValue(instance,
                        ConvertOracleParameterToBaseType(prop.NETProperty.PropertyType, reader.GetOracleValue(count++)));
                }
            }

            return instance;

        }

        public string GetDeclareLine(Type type, string parameterName, OracleUdtInfo udtInfo, MetadataOracleNetTypeDefinition metadata = null)
        {
            metadata = metadata ?? OracleTypeNetMetadata;
            var dependenciesCounter = 0;
            var declareLine = new StringBuilder();
            foreach (var prop in metadata.Properties.Where(c => c.PropertyMetadata != null).OrderBy(c => c.Order))
            {
                var subName = parameterName + "_" + dependenciesCounter++;
                declareLine.AppendLine(GetDeclareLine(prop.NETProperty.PropertyType, subName, prop.PropertyMetadata.UDTInfo, prop.PropertyMetadata));
            }

            if (type.IsCollection())
                declareLine.AppendLine($"{parameterName} {udtInfo.FullCollectionName} := {udtInfo.FullCollectionName}();");
            else
                declareLine.AppendLine($"{parameterName} {udtInfo.FullObjectName};");

            return declareLine.ToString();
        }
    }

    internal abstract class MetadataOracleObject : MetadataOracle
    {
        public abstract Task<object> GetValueFromRefCursorAsync(Type type, OracleRefCursor refCursor);
        public abstract object GetValueFromRefCursor(Type type, OracleRefCursor refCursor);
        public abstract string GetRefCursorQuery(int startNumber, string fieldName);
    }
}
