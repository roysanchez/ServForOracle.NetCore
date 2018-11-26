using System;
using System.Collections.Generic;
using System.Linq;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleNetTypeDefinition : MetadataOracleTypeDefinition
    {
        public MetadataOracleNetTypeDefinition(Type type, MetadataOracleTypeDefinition baseMetadataDefinition)
        {
            UDTInfo = baseMetadataDefinition.UDTInfo;
            Properties = 
                from ora in baseMetadataDefinition.Properties
                join net in type.GetProperties() on ora.Name.ToUpper() equals net.Name.ToUpper() into jn
                from net in jn.DefaultIfEmpty()
                select new MetadataOraclePropertyNetTypeDefinition(ora)
                {
                    NETProperty = net
                };
        }

        public new IEnumerable<MetadataOraclePropertyNetTypeDefinition> Properties { get; set; }
    }
}
