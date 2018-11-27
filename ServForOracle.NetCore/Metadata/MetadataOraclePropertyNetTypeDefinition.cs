using System.Reflection;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOraclePropertyNetTypeDefinition : MetadataOracleTypePropertyDefinition
    {
        public MetadataOraclePropertyNetTypeDefinition()
        {

        }
        public MetadataOraclePropertyNetTypeDefinition(MetadataOracleTypePropertyDefinition baseMetadata)
        {
            Name = baseMetadata.Name;
            Order = baseMetadata.Order;
        }

        public PropertyInfo NETProperty { get; set; }
    }
}