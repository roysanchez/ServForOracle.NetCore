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
    public class MetadataOracleObject<T>: MetadataOracle
    {
        private readonly Regex regex;
        private readonly string ConstructorString;

        private readonly MetadataOracleType metadata;
        private readonly Type Type = typeof(T);

        public MetadataOracleObject(string schema, string objectName, string listName, OracleConnection connection)
            : this(schema, objectName, connection)
        {
            metadata.CollectionName = listName;
        }

        public MetadataOracleObject(string schema, string objectName, OracleConnection connection)
        {
            regex = new Regex(Regex.Escape("$"));

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select attr_no, attr_name, attr_type_name from all_type_attrs where owner = "
                + "upper(:1) and type_name = upper(:2)";
            cmd.Parameters.Add(new OracleParameter(":1", schema));
            cmd.Parameters.Add(new OracleParameter(":2", objectName));

            var reader = cmd.ExecuteReader();
            var properties = new List<MetadataOracleTypeProperty>();
            var NETProperties = Type.IsCollection() ? Type.GetCollectionUnderType().GetProperties() : Type.GetProperties();

            while (reader.Read())
            {
                var property = new MetadataOracleTypeProperty
                {
                    Order = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };

                //TODO Buscar por el atributo
                property.NETProperty = NETProperties
                    .Where(c => c.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();

                properties.Add(property);
            }

            metadata = new MetadataOracleType { Properties = properties, ObjectName = objectName, Schema = schema };

            var constructor = new StringBuilder($"{schema}.{objectName}(");

            for (var counter = 0; counter < properties.Count; counter++)
            {
                constructor.Append(properties[counter].Name);
                constructor.Append("=>");

                constructor.Append('$');
                if (counter + 1 < properties.Count)
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
            foreach (var prop in metadata.Properties.OrderBy(c => c.Order))
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
                    foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                    {
                        parameters.Add(new OracleParameter($":{startNumber++}", value != null ? prop.NETProperty.GetValue(temp) : null));
                    }
                }
            }
            else
            {
                foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                {
                    parameters.Add(new OracleParameter($":{startNumber++}", value != null ? prop.NETProperty.GetValue(value) : null));
                }
            }

            return parameters.ToArray();
        }

        public string GetRefCursorCollectionQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber++} for select ");
            var first = true;
            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null))
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

        public string GetRefCursorQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber++} for select ");
            var first = true;
            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null))
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

        public OracleParameter GetOracleParameterForRefCursor(int starNumber)
        {
            return new OracleParameter($":{starNumber}", DBNull.Value)
            {
                OracleDbType = OracleDbType.RefCursor
            };
        }

        public T GetValueFromRefCursor(OracleRefCursor refCursor)
        {
            dynamic instance = Type.CreateInstance();

            var reader = refCursor.GetDataReader();

            if (Type.IsCollection())
            {
                while (reader.Read())
                {
                    dynamic tempValue = Type.GetCollectionUnderType().CreateInstance();
                    var count = 0;
                    foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                    {
                        prop.NETProperty.SetValue(tempValue,
                        ConvertOracleParameterToBaseType(prop.NETProperty.PropertyType, reader.GetOracleValue(count++)));
                    }

                    instance.Add(tempValue);
                }

                return Type.IsArray ? Enumerable.ToArray(instance) : Enumerable.AsEnumerable(instance);
            }
            else
            {
                while (reader.Read())
                {
                    var count = 0;
                    foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                    {
                        prop.NETProperty.SetValue(instance,
                            ConvertOracleParameterToBaseType(prop.NETProperty.PropertyType, reader.GetOracleValue(count++)));
                    }
                }

                return (T)instance;
            }
        }

        public T[] GetListValueFromRefCursor(OracleRefCursor refCursor)
        {
            var list = new List<T>();

            var reader = refCursor.GetDataReader();
            while (reader.Read())
            {
                var count = 0;
                var instance = Type.CreateInstance();
                foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                {
                    prop.NETProperty.SetValue(instance,
                        ConvertOracleParameterToBaseType(prop.NETProperty.PropertyType, reader.GetOracleValue(count++)));
                }
                list.Add((T)instance);
            }

            return list.ToArray();
        }
    }
}
