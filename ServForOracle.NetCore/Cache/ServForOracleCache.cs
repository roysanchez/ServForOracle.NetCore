using Microsoft.Extensions.Caching.Memory;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Reflection;

namespace ServForOracle.NetCore.Cache
{
    public class ServForOracleCache: IServForOracleCache
    {
        public IMemoryCache Cache { get; private set; }

        private static ServForOracleCache _servForOracleCache;
        private static readonly object Padlock = new object();

        internal ServForOracleCache(IMemoryCache memoryCache)
        {
            Cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public static ServForOracleCache Create(IMemoryCache memoryCache)
        {
            if (_servForOracleCache is null)
            {
                lock (Padlock)
                {
                    if(_servForOracleCache is null)
                    {
                        _servForOracleCache = new ServForOracleCache(memoryCache);
                    }
                }
            }

            return _servForOracleCache;
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

        internal virtual void AddOracleUDTPresets(Type type, OracleUdtInfo info, UdtPropertyNetPropertyMap[] props, bool fuzzyNameMatch = true)
        {
            SaveUdtInfo(type.FullName, info, props, fuzzyNameMatch);
        }

        internal virtual MetadataBase GetMetadata(string name)
        {
            Cache.TryGetValue($"metadata-{name}", out MetadataBase metadata);
            return metadata;
        }

        internal virtual void SaveMetadata(string name, MetadataBase metadata)
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
