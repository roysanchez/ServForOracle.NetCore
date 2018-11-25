using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleTypeDefinition
    {
        public virtual IEnumerable<MetadataOraclePropertyTypeDefinition> Properties { get; set; }
        public string Schema { get; set; }
        public string ObjectName { get; set; }
        public string CollectionName { get; set; }

        public string FullObjectName => $"{Schema}.{ObjectName}";
        public string FullCollectionName => $"{Schema}.{ObjectName}";
    }
}
