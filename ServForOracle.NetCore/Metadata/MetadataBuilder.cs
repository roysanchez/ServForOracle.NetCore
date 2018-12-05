using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataBuilder
    {
        public OracleConnection OracleConnection { get; set; }
        public static ConcurrentBag<MetadataOracleTypeDefinition> OracleUDTs { get; private set; }
        public static ConcurrentDictionary<Type, MetadataOracle> TypeDefinitionsOracleUDT { get; private set; }
        public static ConcurrentDictionary<Type, (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props)> PresetUDTs { get; private set; }

        static MetadataBuilder()
        {
            OracleUDTs = new ConcurrentBag<MetadataOracleTypeDefinition>();
            TypeDefinitionsOracleUDT = new ConcurrentDictionary<Type, MetadataOracle>();
            PresetUDTs = new ConcurrentDictionary<Type, (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props)>();
        }
        internal static void AddOracleUDTPresets(Type Type, OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props)
        {
                PresetUDTs.TryAdd(Type, (Info, Props));
        }

        public MetadataBuilder(OracleConnection connection)
        {
            OracleConnection = connection;
        }

        public async Task<MetadataOracleObject<T>> GetOrRegisterMetadataOracleObjectAsync<T>(OracleUdtInfo udtInfo)
        {
            var type = typeof(T);
            TypeDefinitionsOracleUDT.TryGetValue(type, out MetadataOracle metadata);

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

        public MetadataOracleObject<T> GetOrRegisterMetadataOracleObject<T>(OracleUdtInfo udtInfo)
        {
            var type = typeof(T);
            TypeDefinitionsOracleUDT.TryGetValue(type, out MetadataOracle metadata);

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

        /// <see cref="MetadataOracleObject{T}.MetadataOracleObject(MetadataOracleTypeDefinition, UdtPropertyNetPropertyMap[])>"
        private async Task<object> RegisterAsync(Type type, OracleConnection con, OracleUdtInfo udtInfo)
        {
            var metadataGenericType = typeof(MetadataOracleObject<>).MakeGenericType(type);
            var typeMetadata = await GetOrCreateOracleTypeMetadataAsync(con, udtInfo);

            var metadata = metadataGenericType.CreateInstance(typeMetadata, GetPresetProperties(type));

            TypeDefinitionsOracleUDT.TryAdd(type, metadata as MetadataOracle);

            return metadata;
        }

        /// <see cref="MetadataOracleObject{T}.MetadataOracleObject(MetadataOracleTypeDefinition, UdtPropertyNetPropertyMap[])>"
        private object Register(Type type, OracleConnection con, OracleUdtInfo udtInfo)
        {
            var metadataGenericType = typeof(MetadataOracleObject<>).MakeGenericType(type);
            var typeMetadata = GetOrCreateOracleTypeMetadata(con, udtInfo);

            var metadata = metadataGenericType.CreateInstance(typeMetadata, GetPresetProperties(type));

            TypeDefinitionsOracleUDT.TryAdd(type, metadata as MetadataOracle);

            return metadata;
        }

        //GetValueOrDefault doesn't exists in net standard
        public static (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props) PresetGetValueOrDefault(Type type)
        {
            PresetUDTs.TryGetValue(type, out var preset);
            return preset;
        }

        private UdtPropertyNetPropertyMap[] GetPresetProperties(Type type)
        {
            if (type.IsCollection())
            {
                return PresetGetValueOrDefault(type.GetCollectionUnderType()).Props;
            }
            else
            {
                return PresetGetValueOrDefault(type).Props;
            }
        }

        private async Task<object> RegisterAsync(Type type, OracleConnection con)
        {
            var udtInfo = GetUDTInfo(type);
            return await RegisterAsync(type, con, udtInfo);
        }

        private object Register(Type type, OracleConnection con)
        {
            var udtInfo = GetUDTInfo(type);
            return Register(type, con, udtInfo);
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

        private async Task<MetadataOracleTypeDefinition> GetOrCreateOracleTypeMetadataAsync(OracleConnection connection, OracleUdtInfo udtInfo)
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

        private MetadataOracleTypeDefinition GetOrCreateOracleTypeMetadata(OracleConnection connection, OracleUdtInfo udtInfo)
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

        private OracleCommand CreateCommand(OracleConnection connection, OracleUdtInfo udtInfo)
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

        private OracleUdtInfo GetOracleCollectionUnderlyingType(OracleConnection con, string schema, string collectionName)
        {
            //TODO make recursive
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

            var reader = cmd.ExecuteReader();

            while(reader.Read())
            {
                return new OracleUdtInfo(reader.GetString(0), reader.GetString(1), schema, collectionName);
            }

            return null;
        }

        private List<MetadataOracleTypePropertyDefinition> ExecuteReaderAndLoadTypeDefinition(OracleCommand cmd)
        {
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
                    if (reader.GetString(4) == "COLLECTION")
                    {
                        var property = new MetadataOracleTypeSubTypeDefinition
                        {
                            Order = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            MetadataOracleType = GetOrCreateOracleTypeMetadata(cmd.Connection, GetOracleCollectionUnderlyingType(cmd.Connection, reader.GetString(2), reader.GetString(3)))
                        };
                        properties.Add(property);
                    }
                    else
                    {
                        var property = new MetadataOracleTypeSubTypeDefinition
                        {
                            Order = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            MetadataOracleType = GetOrCreateOracleTypeMetadata(cmd.Connection, new OracleUdtInfo(reader.GetString(2), reader.GetString(3)))
                        };
                        properties.Add(property);
                    }
                }
            }

            return properties;
        }

        private async Task<List<MetadataOracleTypePropertyDefinition>> ExecuteReaderAndLoadTypeDefinitionAsync(OracleCommand cmd)
        {
            var reader = cmd.ExecuteReader();
            var properties = new List<MetadataOracleTypePropertyDefinition>();

            while (await reader.ReadAsync())
            {
                var property = new MetadataOracleTypePropertyDefinition
                {
                    Order = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };

                properties.Add(property);
            }

            return properties;
        }
    }
}
