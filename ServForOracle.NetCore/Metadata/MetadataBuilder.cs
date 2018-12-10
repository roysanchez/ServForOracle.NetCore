using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataBuilder: MetadataBase
    {
        private const string COLLECTION = "COLLECTION";

        public DbConnection OracleConnection { get; private set; }

        public MetadataBuilder(DbConnection connection)
        {
            if(connection is null || string.IsNullOrWhiteSpace(connection.ConnectionString))
            {
                throw new ArgumentNullException(nameof(connection));
            }

            OracleConnection = connection;
        }

        public virtual async Task<MetadataOracleObject<T>> GetOrRegisterMetadataOracleObjectAsync<T>(OracleUdtInfo udtInfo)
        {
            GetTypeAndCachedMetadata<T>(out var type, out var metadata);

            if (metadata == null)
            {
                if (udtInfo != null)
                {
                    return await RegisterAsync(type, OracleConnection, udtInfo) as MetadataOracleObject<T>;
                }
                else
                {
                    return await RegisterAsync(type, OracleConnection) as MetadataOracleObject<T>;
                }
            }
            else
            {
                return metadata as MetadataOracleObject<T>;
            }
        }

        public virtual MetadataOracleObject<T> GetOrRegisterMetadataOracleObject<T>(OracleUdtInfo udtInfo)
        {
            GetTypeAndCachedMetadata<T>(out var type, out var metadata);

            if (metadata == null)
            {
                if (udtInfo != null)
                {
                    return Register(type, OracleConnection, udtInfo) as MetadataOracleObject<T>;
                }
                else
                {
                    return Register(type, OracleConnection) as MetadataOracleObject<T>;
                }
            }
            else
            {
                return metadata as MetadataOracleObject<T>;
            }
        }

        private void GetTypeAndCachedMetadata<T>(out Type type, out MetadataOracle metadata)
        {
            type = typeof(T);
            TypeDefinitionsOracleUDT.TryGetValue(type, out metadata);
        }

        private async Task<object> RegisterAsync(Type type, DbConnection con)
        {
            var udtInfo = GetUDTInfo(type);
            return await RegisterAsync(type, con, udtInfo);
        }

        /// <see cref="MetadataOracleObject{T}.MetadataOracleObject(MetadataOracleTypeDefinition, UdtPropertyNetPropertyMap[], bool)"
        private async Task<object> RegisterAsync(Type type, DbConnection con, OracleUdtInfo udtInfo)
        {
            var metadataGenericType = typeof(MetadataOracleObject<>).MakeGenericType(type);
            var typeMetadata = await GetOrCreateOracleTypeMetadataAsync(con, udtInfo);

            var (_, props, fuzzyMatch) = PresetGetValueOrDefault(type);
            var metadata = metadataGenericType.CreateInstance(typeMetadata, props, fuzzyMatch);

            TypeDefinitionsOracleUDT.TryAdd(type, metadata as MetadataOracle);

            return metadata;
        }

        private object Register(Type type, DbConnection con)
        {
            var udtInfo = GetUDTInfo(type);
            return Register(type, con, udtInfo);
        }

        /// <see cref="MetadataOracleObject{T}.MetadataOracleObject(MetadataOracleTypeDefinition, UdtPropertyNetPropertyMap[], bool)"
        private object Register(Type type, DbConnection con, OracleUdtInfo udtInfo)
        {
            var metadataGenericType = typeof(MetadataOracleObject<>).MakeGenericType(type);
            var typeMetadata = GetOrCreateOracleTypeMetadata(con, udtInfo);

            var (_, props, fuzzyMatch) = PresetGetValueOrDefault(type);
            var metadata = metadataGenericType.CreateInstance(typeMetadata, props, fuzzyMatch);

            TypeDefinitionsOracleUDT.TryAdd(type, metadata as MetadataOracle);

            return metadata;
        }

        private OracleUdtInfo GetUDTInfo(Type type)
        {
            OracleUdtInfo udtInfo;
            if (type.IsCollection())
            {
                var underType = type.GetCollectionUnderType();
                udtInfo = underType.GetCustomAttribute<OracleUdtAttribute>()?.UDTInfo
                    ?? PresetGetValueOrDefault(underType).Info;
            }
            else
            {
                udtInfo = type.GetCustomAttribute<OracleUdtAttribute>()?.UDTInfo
                    ?? PresetGetValueOrDefault(type).Info;
            }

            if (udtInfo == null)
            {
                throw new ArgumentException($"The type {type.FullName} needs to have the {nameof(OracleUdtAttribute)}" +
                    $" attribute set or pass the {nameof(OracleUdtInfo)} class to the execute method.");

            }

            return udtInfo;
        }

        private async Task<MetadataOracleTypeDefinition> GetOrCreateOracleTypeMetadataAsync(DbConnection connection, OracleUdtInfo udtInfo)
        {
            if (OracleConnection.State != ConnectionState.Open)
            {
                await OracleConnection.OpenAsync();
            }

            var exists = OracleUDTs.FirstOrDefault(c => c.UDTInfo.Equals(udtInfo));
            if (exists != null)
                return exists;

            var cmd = CreateCommand(connection, udtInfo);

            var properties = await ExecuteReaderAndLoadTypeDefinitionAsync(cmd);
            return CreateAndSaveMetadata(udtInfo, properties);
        }

        private MetadataOracleTypeDefinition GetOrCreateOracleTypeMetadata(DbConnection connection, OracleUdtInfo udtInfo)
        {
            if (OracleConnection.State != ConnectionState.Open)
            {
                OracleConnection.Open();
            }

            var exists = OracleUDTs.FirstOrDefault(c => c.UDTInfo.Equals(udtInfo));
            if (exists != null)
                return exists;

            var cmd = CreateCommand(connection, udtInfo);

            var properties = ExecuteReaderAndLoadTypeDefinition(cmd);
            return CreateAndSaveMetadata(udtInfo, properties);
        }

        private MetadataOracleTypeDefinition CreateAndSaveMetadata(OracleUdtInfo udtInfo, List<MetadataOracleTypePropertyDefinition> properties)
        {
            var metadata = new MetadataOracleTypeDefinition
            {
                Properties = properties,
                UDTInfo = udtInfo
            };

            OracleUDTs.Add(metadata);

            return metadata;
        }

        private List<MetadataOracleTypePropertyDefinition> ExecuteReaderAndLoadTypeDefinition(DbCommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            var reader = cmd.ExecuteReader();
            var properties = new List<MetadataOracleTypePropertyDefinition>();

            while (reader.Read())
            {
                if (reader.IsDBNull(2))
                {
                    var property = new MetadataOracleTypePropertyDefinition
                    {
                        Order = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };

                    properties.Add(property);
                }
                else
                {
                    var property = new MetadataOracleTypeSubTypeDefinition
                    {
                        Order = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };

                    if (reader.GetString(4) == COLLECTION)
                    {
                        property.MetadataOracleType = GetOrCreateOracleTypeMetadata(cmd.Connection, GetOracleCollectionUnderlyingType(cmd.Connection, reader.GetString(2), reader.GetString(3)));
                    }
                    else
                    {
                        property.MetadataOracleType = GetOrCreateOracleTypeMetadata(cmd.Connection, new OracleUdtInfo(reader.GetString(2), reader.GetString(3), isCollection: false));
                    }

                    properties.Add(property);
                }
            }


            return properties;
        }

        private async Task<List<MetadataOracleTypePropertyDefinition>> ExecuteReaderAndLoadTypeDefinitionAsync(DbCommand cmd)
        {
            var reader = await cmd.ExecuteReaderAsync();
            var properties = new List<MetadataOracleTypePropertyDefinition>();

            while (await reader.ReadAsync())
            {
                if (await reader.IsDBNullAsync(2))
                {
                    var property = new MetadataOracleTypePropertyDefinition
                    {
                        Order = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };

                    properties.Add(property);
                }
                else
                {
                    var property = new MetadataOracleTypeSubTypeDefinition
                    {
                        Order = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };

                    if (reader.GetString(4) == COLLECTION)
                    {
                        property.MetadataOracleType = await GetOrCreateOracleTypeMetadataAsync(cmd.Connection, await GetOracleCollectionUnderlyingTypeAsync(cmd.Connection, reader.GetString(2), reader.GetString(3)));
                    }
                    else
                    {
                        property.MetadataOracleType = await GetOrCreateOracleTypeMetadataAsync(cmd.Connection, new OracleUdtInfo(reader.GetString(2), reader.GetString(3), isCollection: false));
                    }

                    properties.Add(property);
                }
            }

            return properties;
        }

        private OracleUdtInfo GetOracleCollectionUnderlyingType(DbConnection con, string schema, string collectionName)
        {
            var cmd = CreateDbCommandCollectionUnderType(con, schema, collectionName);

            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                if(reader.IsDBNull(2) || reader.GetString(2) != COLLECTION)
                {
                    return new OracleUdtInfo(schema, collectionName, new OracleUdtInfo(reader.GetString(0), reader.GetString(1), isCollection: false));
                }
                else
                {
                    return new OracleUdtInfo(schema, collectionName, GetOracleCollectionUnderlyingType(con, reader.GetString(0), reader.GetString(1)));
                }
            }
            else
            {
                //TODO Log error
                return null;
            }
        }

        private async Task<OracleUdtInfo> GetOracleCollectionUnderlyingTypeAsync(DbConnection con, string schema, string collectionName)
        {
            var cmd = CreateDbCommandCollectionUnderType(con, schema, collectionName);

            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                if (await reader.IsDBNullAsync(2) || reader.GetString(2) != COLLECTION)
                {
                    return new OracleUdtInfo(schema, collectionName, new OracleUdtInfo(reader.GetString(0), reader.GetString(1), isCollection: false));
                }
                else
                {
                    return new OracleUdtInfo(schema, collectionName, await GetOracleCollectionUnderlyingTypeAsync(con, reader.GetString(0), reader.GetString(1)));
                }
            }
            else
            {
                //TODO Log error
                return null;
            }
        }

        private DbCommand CreateCommand(DbConnection connection, OracleUdtInfo udtInfo)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText =
            "select ata.attr_no, ata.attr_name, ata.attr_type_owner, ata.attr_type_name, aty.typecode "
            + "from all_type_attrs ata "
            + "left join all_types aty "
            + "on aty.owner = ata.attr_type_owner "
            + "and aty.type_name = ata.attr_type_name "
            + "where ata.owner = "
                + "upper(:1) and ata.type_name = upper(:2)";
            cmd.Parameters.Add(new OracleParameter(":1", udtInfo.ObjectSchema));
            cmd.Parameters.Add(new OracleParameter(":2", udtInfo.ObjectName));
            return cmd;
        }

        private DbCommand CreateDbCommandCollectionUnderType(DbConnection con, string schema, string collectionName)
        {
            var cmd = con.CreateCommand();
            cmd.CommandText =
                "select act.elem_type_owner, act.elem_type_name, aty.typecode "
                + "from ALL_COLL_TYPES act "
                + "join all_types aty "
                + "on aty.owner = act.owner "
                + "and aty.type_name = act.type_name "
                + "where act.owner = upper(:0) and act.type_name = upper(:1)";

            cmd.Parameters.Add(new OracleParameter(":1", schema));
            cmd.Parameters.Add(new OracleParameter(":2", collectionName));
            return cmd;
        }
    }
}