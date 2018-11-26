using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleTypeDefinition
    {
        public virtual IEnumerable<MetadataOraclePropertyTypeDefinition> Properties { get; set; }
        public OracleUDTInfo UDTInfo { get; set; }
    }
}
