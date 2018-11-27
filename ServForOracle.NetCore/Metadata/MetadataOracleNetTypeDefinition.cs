using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleNetTypeDefinition : MetadataOracleTypeDefinition
    {
        private UDTPropertyNetPropertyMap[] GetUDTPropertyNames(PropertyInfo[] properties)
        {
            return properties.Select(c =>
            {
                var attribute = c.GetCustomAttribute<OracleUDTPropertyAttribute>();
                if (attribute != null)
                    return new UDTPropertyNetPropertyMap(c.Name, attribute.PropertyName);
                else
                    return null;
            })
            .Where(c => c != null)
            .ToArray();
        }

        public MetadataOracleNetTypeDefinition(Type type, MetadataOracleTypeDefinition baseMetadataDefinition,
           UDTPropertyNetPropertyMap[] presetProperties)
        {
            UDTInfo = baseMetadataDefinition.UDTInfo;

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
                
                list.Add(new MetadataOraclePropertyNetTypeDefinition(prop)
                {
                    NETProperty = netProperty
                });
            }

            Properties = list;
        }

        public new IEnumerable<MetadataOraclePropertyNetTypeDefinition> Properties { get; set; }
    }
}
