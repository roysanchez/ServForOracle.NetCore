using System.Collections.Generic;

namespace ServForOracle.NetCore.Metadata
{
    public class MetadataOracleType
    {
        public IEnumerable<MetadataOracleTypeProperty> Properties { get; set; }
        public string Schema { get; set; }
        public string ObjectName { get; set; }
        public string CollectionName { get; set; }
    }
}
