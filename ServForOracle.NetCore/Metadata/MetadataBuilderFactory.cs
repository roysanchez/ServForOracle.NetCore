using Microsoft.Extensions.Logging;
using ServForOracle.NetCore.Cache;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal interface IMetadataBuilderFactory
    {
        MetadataBuilder Create(DbConnection connection);
    }

    internal class MetadataBuilderFactory: IMetadataBuilderFactory
    {
        private readonly ServForOracleCache _cache;
        private readonly ILogger _logger;

        public MetadataBuilderFactory(ServForOracleCache cache, ILogger logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
        }

        public MetadataBuilder Create(DbConnection connection)
        {
            return new MetadataBuilder(connection, _cache, _logger);
        }
    }
}
