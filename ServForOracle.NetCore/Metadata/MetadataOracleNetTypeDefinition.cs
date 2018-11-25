using System;
using System.Collections.Generic;
using System.Linq;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleNetTypeDefinition : MetadataOracleTypeDefinition
    {
        public MetadataOracleNetTypeDefinition(Type type, MetadataOracleTypeDefinition baseMetadataDefinition)
        {
            var properties = new List<MetadataOraclePropertyNetTypeDefinition>();
            foreach(var prop in type.GetProperties())
            {
                var baseProp = baseMetadataDefinition.Properties
                    .Where(c => c.Name.Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
                if (baseProp != null)
                {
                    properties.Add(new MetadataOraclePropertyNetTypeDefinition(baseProp)
                    {
                        NETProperty = prop
                    });
                }
            }

            Properties = properties;
        }

        public new IEnumerable<MetadataOraclePropertyNetTypeDefinition> Properties { get; set; }
    }
}
