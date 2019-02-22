using Microsoft.Extensions.Logging;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Config
{
    public class ConfigurePresetMappings
    {
        private readonly ILogger<ConfigurePresetMappings> _logger;
        public ConfigurePresetMappings()
        {

        }

        public ConfigurePresetMappings(ILogger<ConfigurePresetMappings> logger)
        {
            _logger = logger;
        }

        public void AddOracleUDT(params PresetMap[] presets)
        {
            if (presets != null)
            {
                foreach (var p in presets)
                {
                    MetadataBuilder.AddOracleUDTPresets(p.Type, p.Info, p.ReplacedProperties);
                }
            }
            else
            {
                _logger?.LogWarning("the presets object is null");
            }
        }
    }
}
