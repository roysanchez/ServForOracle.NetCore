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

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleObject<T> : MetadataOracleObject
    {
        private readonly Regex regex;
        private readonly string ConstructorString;
        private readonly Type Type;
        internal readonly MetadataOracleNetTypeDefinition OracleTypeNetMetadata;

        public MetadataOracleObject(MetadataOracleTypeDefinition metadataOracleType, UDTPropertyNetPropertyMap[] customProperties)
        {
            Type = typeof(T);
            if (Type.IsCollection())
            {
                OracleTypeNetMetadata = new MetadataOracleNetTypeDefinition(Type.GetCollectionUnderType(), metadataOracleType,
                    customProperties ?? new UDTPropertyNetPropertyMap[] { });
            }
            else
            {
                OracleTypeNetMetadata = new MetadataOracleNetTypeDefinition(Type, metadataOracleType,
                    customProperties ?? new UDTPropertyNetPropertyMap[] { });
            }

            regex = new Regex(Regex.Escape("$"));

            var constructor = new StringBuilder($"{metadataOracleType.UDTInfo.FullObjectName}(");
            var properties = metadataOracleType.Properties.ToArray();

            for (var counter = 0; counter < properties.Count(); counter++)
            {
                constructor.Append(properties[counter].Name);
                constructor.Append("=>");

                constructor.Append('$');
                if (counter + 1 < properties.Count())
                {
                    constructor.Append(',');
                }
            }
            constructor.Append(");");

            ConstructorString = constructor.ToString();
        }

        private string BuildConstructor(ref int startNumber)
        {
            var constructor = ConstructorString;
            foreach (var prop in OracleTypeNetMetadata.Properties.OrderBy(c => c.Order))
            {
                if (prop.NETProperty != null)
                {
                    constructor = regex.Replace(constructor, $":{startNumber++}", 1);
                }
                else
                {
                    constructor = regex.Replace(constructor, "null", 1);
                }

            }

            return constructor;
        }

        public (string Constructor, int LastNumber) BuildQueryConstructorString(T value, string name, int startNumber)
        {
            var baseString = new StringBuilder();

            if (Type.IsCollection())
            {
                if (value != null)
                {
                    foreach (var v in value as IEnumerable)
                    {
                        var baseConstructor = BuildConstructor(ref startNumber);
                        baseString.AppendLine($"{name}.extend;");
                        baseString.AppendLine($"{name}({name}.last) := {baseConstructor}");
                    }
                }
            }
            else
            {
                var baseConstructor = BuildConstructor(ref startNumber);
                baseString.AppendLine($"{name} := {baseConstructor}");
            }

            return (baseString.ToString(), startNumber);
        }

        public OracleParameter[] GetOracleParameters(T value, int startNumber)
        {
            var parameters = new List<OracleParameter>();

            if (Type.IsCollection())
            {
                if (value != null)
                {
                    foreach (var temp in value as IEnumerable)
                    {
                        foreach (var prop in OracleTypeNetMetadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                        {
                            parameters.Add(new OracleParameter($":{startNumber++}", temp != null ? prop.NETProperty.GetValue(temp) : null));
                        }
                    }
                }
            }
            else
            {
                foreach (var prop in OracleTypeNetMetadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                {
                    parameters.Add(new OracleParameter($":{startNumber++}", value != null ? prop.NETProperty.GetValue(value) : null));
                }
            }

            return parameters.ToArray();
        }

        private string GetRefCursorCollectionQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber} for select ");
            var first = true;
            foreach (var prop in OracleTypeNetMetadata.Properties.Where(c => c.NETProperty != null))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(",");
                }
                query.Append($"c.{prop.Name}");
            }
            query.Append($" from table({fieldName}) c;");

            return query.ToString();
        }

        private string GetRefCursorObjectQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber} for select ");
            var first = true;
            foreach (var prop in OracleTypeNetMetadata.Properties.Where(c => c.NETProperty != null))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(",");
                }
                query.Append($"{fieldName}.{prop.Name}");
            }
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

            var reader = refCursor.GetDataReader();

            if (type.IsCollection())
            {
                var subType = type.GetCollectionUnderType();
                while (await reader.ReadAsync())
                {
                    instance.Add(ReadObjectInstance(subType, reader));
                }

                return type.IsArray ? Enumerable.ToArray(instance) : Enumerable.AsEnumerable(instance);
            }
            else
            {
                while (await reader.ReadAsync())
                {
                    ReadObjectInstance(type, reader);
                }

                return (T)instance;
            }
        }

        public override object GetValueFromRefCursor(Type type, OracleRefCursor refCursor)
        {
            dynamic instance = type.CreateInstance();

            var reader = refCursor.GetDataReader();

            if (type.IsCollection())
            {
                var subType = type.GetCollectionUnderType();
                while (reader.Read())
                {
                    instance.Add(ReadObjectInstance(subType, reader));
                }

                return type.IsArray ? Enumerable.ToArray(instance) : Enumerable.AsEnumerable(instance);
            }
            else
            {
                while (reader.Read())
                {
                    ReadObjectInstance(type, reader);
                }

                return (T)instance;
            }
        }

        private dynamic ReadObjectInstance(Type type, OracleDataReader reader)
        {
            int count = 0;
            var instance = type.CreateInstance();
            foreach (var prop in OracleTypeNetMetadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
            {
                prop.NETProperty.SetValue(instance,
                    ConvertOracleParameterToBaseType(prop.NETProperty.PropertyType, reader.GetOracleValue(count++)));
            }

            return instance;

        }

        public string GetDeclareLine(Type type, string parameterName, OracleUDTInfo udtInfo)
        {
            if (type.IsCollection())
                return $"{parameterName} {udtInfo.FullCollectionName} := {udtInfo.FullCollectionName}();";
            else
                return $"{parameterName} {udtInfo.FullObjectName};";
        }
    }

    internal abstract class MetadataOracleObject : MetadataOracle
    {
        public abstract Task<object> GetValueFromRefCursorAsync(Type type, OracleRefCursor refCursor);
        public abstract object GetValueFromRefCursor(Type type, OracleRefCursor refCursor);
        public abstract string GetRefCursorQuery(int startNumber, string fieldName);
    }
}
