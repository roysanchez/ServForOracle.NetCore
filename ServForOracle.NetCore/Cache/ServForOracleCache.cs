using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ServForOracle.NetCore.Cache
{
    public class ServForOracleCache: IServForOracleCache
    {
        public IMemoryCache Cache { get; private set; }

        private static ServForOracleCache servForOracleCache;

        internal ServForOracleCache(IMemoryCache memoryCache)
        {
            Cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public static ServForOracleCache Create(IMemoryCache memoryCache)
        {
            if (servForOracleCache is null)
            {
                servForOracleCache = new ServForOracleCache(memoryCache);
            }

            return servForOracleCache;
        }

        internal virtual void SaveUdtInfo(string name, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch)
        {
            using (var entry = Cache.CreateEntry($"udt-{name}"))
            {
                entry.SetValue((info, props, fuzzyNameMatch));
            }
        }

        internal virtual (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) GetOtherUdtInfo(string name)
        {
            Cache.TryGetValue($"udt-{name}", out (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) preset);
            return preset;
        }

        internal virtual (OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool FuzzyMatch) PresetGetValueOrDefault(Type type)
        {
            if (type.IsCollection())
            {
                return GetOtherUdtInfo(type.GetCollectionUnderType().FullName);
            }
            else
            {
                return GetOtherUdtInfo(type.FullName);
            }
        }

        internal virtual void AddOracleUDTPresets(Type Type, OracleUdtInfo Info, UdtPropertyNetPropertyMap[] Props, bool fuzzyNameMatch = true)
        {
            SaveUdtInfo(Type.FullName, Info, Props, fuzzyNameMatch);
        }

        internal virtual MetadataOracle GetMetadata(string name)
        {
            Cache.TryGetValue($"matadata-{name}", out MetadataOracle metadata);
            return metadata;
        }

        internal virtual void SaveMetadata(string name, MetadataOracle metadata)
        {
            using (var entry = Cache.CreateEntry($"metadata-{name}"))
            {
                entry.SetValue(metadata);
            }
        }

        internal virtual void SaveTypeDefinition(MetadataOracleTypeDefinition def)
        {
            using (var entry = Cache.CreateEntry($"def-{def.UDTInfo.FullObjectName}"))
            {
                entry.SetValue(def);
            }
        }

        internal virtual MetadataOracleTypeDefinition GetTypeDefinition(string fullObjectName)
        {
            Cache.TryGetValue($"def-{fullObjectName}", out MetadataOracleTypeDefinition info);
            return info;
        }

        internal virtual OracleUdtInfo GetUdtInfoFromAttributeOrPresetCache(Type type)
        {
            if (type.IsCollection())
            {
                var underType = type.GetCollectionUnderType();
                return underType.GetCustomAttribute<OracleUdtAttribute>()?.UDTInfo
                    ?? PresetGetValueOrDefault(underType).Info;
            }
            else
            {
                return type.GetCustomAttribute<OracleUdtAttribute>()?.UDTInfo
                    ?? PresetGetValueOrDefault(type).Info;
            }
        }
    }
}
