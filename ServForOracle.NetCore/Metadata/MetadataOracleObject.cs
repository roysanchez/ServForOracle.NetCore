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

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleObject<T>: MetadataOracleObject
    {
        private readonly Regex regex;
        private readonly string ConstructorString;
        private readonly Type Type;
        internal readonly MetadataOracleNetTypeDefinition OracleTypeNetMetadata;

        public MetadataOracleObject(MetadataOracleTypeDefinition metadataOracleType)
        {
            Type = typeof(T);
            OracleTypeNetMetadata = new MetadataOracleNetTypeDefinition(Type, metadataOracleType);

            regex = new Regex(Regex.Escape("$"));

            var constructor = new StringBuilder($"{metadataOracleType.FullObjectName}(");
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

        public (string Constructor, int LastNumber) BuildConstructor(T value, int startNumber)
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

            return (constructor, startNumber);
        }

        public OracleParameter[] GetOracleParameters(T value, int startNumber)
        {
            var parameters = new List<OracleParameter>();

            if (Type.IsCollection())
            {
                foreach (var temp in value as IEnumerable)
                {
                    foreach (var prop in OracleTypeNetMetadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                    {
                        parameters.Add(new OracleParameter($":{startNumber++}", value != null ? prop.NETProperty.GetValue(temp) : null));
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

        public override string GetRefCursorCollectionQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber++} for select ");
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

        public override string GetRefCursorQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber++} for select ");
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

        public override object GetValueFromRefCursor(OracleRefCursor refCursor)
        {
            dynamic instance = Type.CreateInstance();
            
            var reader = refCursor.GetDataReader();

            if (Type.IsCollection())
            {
                var subType = Type.GetCollectionUnderType();
                while (reader.Read())
                {
                    instance.Add(ReadObjectInstance(subType, reader));
                }

                return Type.IsArray ? Enumerable.ToArray(instance) : Enumerable.AsEnumerable(instance);
            }
            else
            {
                while (reader.Read())
                {
                    ReadObjectInstance(Type, reader);
                }

                return (T)instance;
            }
        }

        private object ReadObjectInstance(Type type, OracleDataReader reader)
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
    }

    internal abstract class MetadataOracleObject: MetadataOracle
    {
        public abstract object GetValueFromRefCursor(OracleRefCursor refCursor);
        public abstract string GetRefCursorQuery(int startNumber, string fieldName);
        public abstract string GetRefCursorCollectionQuery(int startNumber, string fieldName);
    }
}
