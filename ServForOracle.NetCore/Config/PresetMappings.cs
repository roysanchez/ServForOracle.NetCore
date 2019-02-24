using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Config
{
    public class ConfigurePresetMappings
    {
        private readonly ILogger<ConfigurePresetMappings> _logger;
        private readonly ServForOracleCache _cache;

        public ConfigurePresetMappings(ILogger<ConfigurePresetMappings> logger, ServForOracleCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public void AddOracleUDT(params PresetMap[] presets)
        {
            if (presets != null)
            {
                foreach (var p in presets)
                {
                    _cache.AddOracleUDTPresets(p.Type, p.Info, p.ReplacedProperties);
                }
            }
            else
            {
                _logger?.LogWarning("the presets object is null");
            }
        }
    }
}
