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
           UdtPropertyNetPropertyMap[] presetProperties)
        {
            UDTInfo = baseMetadataDefinition.UDTInfo;
            presetProperties = presetProperties ?? new UdtPropertyNetPropertyMap[] { };
            var list = new List<MetadataOraclePropertyNetTypeDefinition>();
            var netProperties = type.GetProperties();
            var netAttibutes = GetUDTPropertyNames(netProperties);

            foreach(var prop in baseMetadataDefinition.Properties)
            {
                var netProperty = netProperties.FirstOrDefault(c => c.Name.ToUpper() == prop.Name);
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
                        PropertyMetadata = new MetadataOracleNetTypeDefinition(
                            netProperty.PropertyType, propertyObject.MetadataOracleType, MetadataBuilder.PresetGetValueOrDefault(netProperty.PropertyType).Props)
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

        public new IEnumerable<MetadataOraclePropertyNetTypeDefinition> Properties { get; set; }
    }
}
