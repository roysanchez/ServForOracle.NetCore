using System.Reflection;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOraclePropertyNetTypeDefinition : MetadataOraclePropertyTypeDefinition
    {
        public MetadataOraclePropertyNetTypeDefinition()
        {

        }
        public MetadataOraclePropertyNetTypeDefinition(MetadataOraclePropertyTypeDefinition baseMetadata)
        {
            Name = baseMetadata.Name;
            Order = baseMetadata.Order;
        }

        public PropertyInfo NETProperty { get; set; }
    }
}