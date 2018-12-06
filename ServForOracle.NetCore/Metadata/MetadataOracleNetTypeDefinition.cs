using ServForOracle.NetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleNetTypeDefinition : MetadataOracleTypeDefinition
    {
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

        public MetadataOracleNetTypeDefinition(Type type, MetadataOracleTypeDefinition baseMetadataDefinition,
           UdtPropertyNetPropertyMap[] presetProperties, bool fuzzyNameMatch)
        {
            UDTInfo = baseMetadataDefinition.UDTInfo;
            presetProperties = presetProperties ?? new UdtPropertyNetPropertyMap[] { };
            var list = new List<MetadataOraclePropertyNetTypeDefinition>();
            var netProperties = type.GetProperties();
            var netAttibutes = GetUDTPropertyNames(netProperties);

            foreach(var prop in baseMetadataDefinition.Properties)
            {
                var netProperty = netProperties.FirstOrDefault(c => c.Name.ToUpper() == prop.Name);
                if(netProperty == null && fuzzyNameMatch)
                {
                    //TODO Do more advance fuzzy match
                    netProperty = netProperties.FirstOrDefault(c => c.Name.ToUpper() == prop.Name.Replace("_", string.Empty));
                }

                var presetNetPropertyName = presetProperties.FirstOrDefault(c => c.UDTPropertyName == prop.Name)?.NetPropertyName;
                var attributePropertyName = netAttibutes.FirstOrDefault(c => c.UDTPropertyName == prop.Name)?.NetPropertyName;

                if(!string.IsNullOrWhiteSpace(attributePropertyName))
                {
                    netProperty = netProperties.FirstOrDefault(c => c.Name.ToUpper() == attributePropertyName);
                }
                else if(!string.IsNullOrWhiteSpace(presetNetPropertyName))
                {
                    netProperty = netProperties.FirstOrDefault(c => c.Name.ToUpper() == presetNetPropertyName);
                }
                else
                {
                    //TODO throw warning
                }

                if (prop is MetadataOracleTypeSubTypeDefinition propertyObject)
                {
                    list.Add(new MetadataOraclePropertyNetTypeDefinition(prop)
                    {
                        NETProperty = netProperty,
                        PropertyMetadata = GetPropertyMetadata(netProperty.PropertyType, propertyObject, fuzzyNameMatch)
                    });
                }
                else
                {
                    list.Add(new MetadataOraclePropertyNetTypeDefinition(prop)
                    {
                        NETProperty = netProperty
                    });
                }
            }

            Properties = list;
        }

        private static MetadataOracleNetTypeDefinition GetPropertyMetadata(Type propertyType, MetadataOracleTypeSubTypeDefinition propertyObject, bool fuzzyNameMatch)
        {
            if (propertyType.IsCollection())
            {
                return new MetadataOracleNetTypeDefinition(
                                            propertyType.GetCollectionUnderType(), propertyObject.MetadataOracleType, MetadataBuilder.PresetGetValueOrDefault(propertyType.GetCollectionUnderType()).Props, fuzzyNameMatch);
            }
            else
            {
                return new MetadataOracleNetTypeDefinition(
                                            propertyType, propertyObject.MetadataOracleType, MetadataBuilder.PresetGetValueOrDefault(propertyType).Props, fuzzyNameMatch);
            }
        }

        public new IEnumerable<MetadataOraclePropertyNetTypeDefinition> Properties { get; set; }
    }
}
