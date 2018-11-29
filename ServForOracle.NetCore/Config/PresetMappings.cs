using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Config
{

    

    public class ConfigurePresetMappings
    {
        public void AddOracleUDT<T>(params PresetMap[] presets)
            => AddOracleUDTConfiguration(typeof(T), presets);

        public void AddOracleUDTConfiguration(Type type, params PresetMap[] presets)
        {
            foreach(var p in presets)
            {
                MetadataBuilder.AddOracleUDTPresets(type, p.Info, p.ReplacedProperties);
            }
        }
    }
}
