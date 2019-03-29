using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal interface IMetadataFactory
    {
        MetadataOracleCommon CreateCommon();
        MetadataOracleBoolean CreateBoolean();
    }

    internal class MetadataFactory: IMetadataFactory
    {
        public MetadataOracleCommon CreateCommon()
        {
            return new MetadataOracleCommon();
        }

        public MetadataOracleBoolean CreateBoolean()
        {
            return new MetadataOracleBoolean();
        }
    }
}
