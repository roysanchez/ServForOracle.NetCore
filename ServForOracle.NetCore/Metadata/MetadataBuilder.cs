using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataBuilder
    {
        public OracleConnection OracleConnection { get; set; }
        public ConcurrentBag<MetadataOracleTypeDefinition> OracleUDTs { get; private set; }
        public ConcurrentDictionary<Type, MetadataOracle> TypeDefinitionsOracleUDT { get; private set; }

        public MetadataBuilder(OracleConnection connection)
        {
            OracleConnection = connection;
            OracleUDTs = new ConcurrentBag<MetadataOracleTypeDefinition>();

            var executing = Assembly.GetExecutingAssembly();

            var assemblies =
                    from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    where assembly != executing
                       && !assembly.GlobalAssemblyCache
                       //&& assembly.Location == executing.Location
                       && !assembly.FullName.StartsWith("Microsoft")
                       && !assembly.FullName.StartsWith("System")
                       && !assembly.FullName.StartsWith("Oracle")
                       && !assembly.FullName.StartsWith("xunit")
                    select assembly;

            var types = assemblies.SelectMany(a => a.GetTypes())
                        .Where(t => t.IsClass && !t.IsSealed && !t.IsAbstract);

            var collections = types.Where(t => t.GetCustomAttribute<UDTCollectionNameAttribute>() != null);

            foreach (var type in collections)
            {
                RegisterByType(type, connection);
            }

            foreach (var type in types.Except(collections))
            {

            }
        }

        public MetadataOracleObject<T> GetOrRegisterMetadataOracleObject<T>(string schema = null, string objectName = null, string collectionName = null)
        {
            var type = typeof(T);
            TypeDefinitionsOracleUDT.TryGetValue(type, out var metadata);

            if (metadata == null)
            {
                return Register(type, OracleConnection, schema, objectName, collectionName) as MetadataOracleObject<T>;
            }
            else
            {
                return metadata as MetadataOracleObject<T>;
            }
        }

        public MetadataOracle GetMetadataOracle()
        {
            return new MetadataOracle();
        }

        private object Register(Type type, OracleConnection con, string objectSchema, string objectName, string collectionName)
        {
            var metadataGenericType = typeof(MetadataOracleObject<>).MakeGenericType(type);
            var metadata = metadataGenericType.CreateInstance(GetOrCreateOracleTypeMetadata(con, objectSchema, objectName, collectionName));

            TypeDefinitionsOracleUDT.TryAdd(type, metadata as MetadataOracle);

            return metadata;
        }

        private object RegisterByType(Type type, OracleConnection con)
        {
            var objectSchema = string.Empty;
            var objectName = type.GetCustomAttribute<UDTNameAttribute>().Name;
            var collectionName = type.GetCustomAttribute<UDTCollectionNameAttribute>()?.Name;

            return Register(type, con, objectSchema, objectName, collectionName);
        }

        private MetadataOracleTypeDefinition GetOrCreateOracleTypeMetadata(OracleConnection connection, string schema, string objectName,
            string collectionName)
        {
            if (OracleConnection.State != ConnectionState.Open)
            {
                OracleConnection.Open();
            }

            var checkExists = OracleUDTs.FirstOrDefault(c => c.Schema == schema && c.ObjectName == objectName);
            if (checkExists != null)
                return checkExists;

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select attr_no, attr_name, attr_type_name from all_type_attrs where owner = "
                + "upper(:1) and type_name = upper(:2)";
            cmd.Parameters.Add(new OracleParameter(":1", schema));
            cmd.Parameters.Add(new OracleParameter(":2", objectName));

            var reader = cmd.ExecuteReader();
            var properties = new List<MetadataOraclePropertyTypeDefinition>();

            while (reader.Read())
            {
                var property = new MetadataOraclePropertyNetTypeDefinition
                {
                    Order = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };

                properties.Add(property);
            }

            var metadata = new MetadataOracleTypeDefinition
            {
                Properties = properties,
                ObjectName = objectName,
                CollectionName = collectionName,
                Schema = schema
            };

            OracleUDTs.Add(metadata);

            return metadata;
        }
    }
}
