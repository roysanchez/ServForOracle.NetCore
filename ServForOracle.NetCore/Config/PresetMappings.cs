using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Config
{

    

    public class ConfigurePresetMappings
    {
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
                //TODO Throw warning
            }
        }
    }
}
