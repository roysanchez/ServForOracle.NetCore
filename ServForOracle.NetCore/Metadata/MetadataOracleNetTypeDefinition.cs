using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleNetTypeDefinition : MetadataOracleTypeDefinition
    {
        public new virtual IEnumerable<MetadataOraclePropertyNetTypeDefinition> Properties { get; set; }

        private readonly Regex Regex = new Regex(Regex.Escape("_"));
        private readonly ServForOracleCache Cache;

        public MetadataOracleNetTypeDefinition(ServForOracleCache cache, Type type, MetadataOracleTypeDefinition baseMetadataDefinition,
           UdtPropertyNetPropertyMap[] presetProperties, bool fuzzyNameMatch)
        {
            if (baseMetadataDefinition == null)
            {
                throw new ArgumentNullException(nameof(baseMetadataDefinition));
            }

            Cache = cache;

            UDTInfo = baseMetadataDefinition.UDTInfo;
            Properties = ProcessPresetNetTypePropertiesMap(type, baseMetadataDefinition, presetProperties, fuzzyNameMatch);
        }

        private IEnumerable<MetadataOraclePropertyNetTypeDefinition> ProcessPresetNetTypePropertiesMap(Type type, MetadataOracleTypeDefinition baseMetadataDefinition, UdtPropertyNetPropertyMap[] presetProperties, bool fuzzyNameMatch)
        {
            presetProperties = presetProperties ?? new UdtPropertyNetPropertyMap[] { };
            var list = new List<MetadataOraclePropertyNetTypeDefinition>();
            var netProperties = type.GetProperties();
            var netAttibutes = GetUDTPropertyNames(netProperties);

            foreach (var prop in baseMetadataDefinition.Properties)
            {
                var netProperty = netProperties.FirstOrDefault(c => string.Equals(c.Name, prop.Name, StringComparison.InvariantCultureIgnoreCase));
                if (netProperty == null && fuzzyNameMatch)
                {
                    //TODO Do more advance fuzzy matching, right now it only replaces the underscore
                    netProperty = netProperties.FirstOrDefault(c => string.Equals(c.Name, Regex.Replace(prop.Name, string.Empty), StringComparison.InvariantCultureIgnoreCase));
                }

                var presetNetPropertyName = presetProperties.FirstOrDefault(c => string.Equals(c.UDTPropertyName, prop.Name, StringComparison.InvariantCultureIgnoreCase))?.NetPropertyName;
                var attributePropertyName = netAttibutes.FirstOrDefault(c => string.Equals(c.UDTPropertyName, prop.Name, StringComparison.InvariantCultureIgnoreCase))?.NetPropertyName;

                if (!string.IsNullOrWhiteSpace(attributePropertyName))
                {
                    netProperty = netProperties.FirstOrDefault(c => string.Equals(c.Name, attributePropertyName, StringComparison.InvariantCultureIgnoreCase));
                }
                else if (!string.IsNullOrWhiteSpace(presetNetPropertyName))
                {
                    netProperty = netProperties.FirstOrDefault(c => string.Equals(c.Name, presetNetPropertyName, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    //TODO throw or log warning
                }

                var propertyTypeDefinition = new MetadataOraclePropertyNetTypeDefinition(prop);

                if (netProperty != null)
                {
                    propertyTypeDefinition.NETProperty = netProperty;
                    if (prop is MetadataOracleTypeSubTypeDefinition propertyObject)
                    {
                        propertyTypeDefinition.PropertyMetadata = GetPropertyMetadata(netProperty.PropertyType, propertyObject, fuzzyNameMatch);
                    }
                }

                list.Add(propertyTypeDefinition);
            }

            return list;
        }

        private UdtPropertyNetPropertyMap[] GetUDTPropertyNames(PropertyInfo[] properties)
        {
            if (properties == null || properties.Length == 0)
                return new UdtPropertyNetPropertyMap[0];

            return properties.Select(c =>
            {
                var attribute = c.GetCustomAttribute<OracleUdtPropertyAttribute>();
                if (attribute != null)
                    return new UdtPropertyNetPropertyMap(c.Name, attribute.PropertyName);
                else
                    return null;
            })
            .Where(c => c != null)
            .ToArray();
        }

        private MetadataOracleNetTypeDefinition GetPropertyMetadata(Type propertyType, MetadataOracleTypeSubTypeDefinition propertyObject, bool fuzzyNameMatch)
        {
            if (propertyType.IsCollection())
            {
                return new MetadataOracleNetTypeDefinition(
                                            Cache,
                                            propertyType.GetCollectionUnderType(),
                                            propertyObject.MetadataOracleType,
                                            Cache.PresetGetValueOrDefault(propertyType.GetCollectionUnderType()).Props,
                                            fuzzyNameMatch);
            }
            else
            {
                return new MetadataOracleNetTypeDefinition(
                                            Cache,
                                            propertyType,
                                            propertyObject.MetadataOracleType,
                                            Cache.PresetGetValueOrDefault(propertyType).Props,
                                            fuzzyNameMatch);
            }
        }
    }
}