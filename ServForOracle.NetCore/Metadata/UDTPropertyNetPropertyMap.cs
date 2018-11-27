using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal class UDTPropertyNetPropertyMap: IEquatable<UDTPropertyNetPropertyMap>
    {
        public UDTPropertyNetPropertyMap(string netPropertyName, string udtPropertyName)
        {
            UDTPropertyName = udtPropertyName.ToUpper();
            NetPropertyName = netPropertyName.ToUpper();
        }

        public string UDTPropertyName { get; private set; }
        public string NetPropertyName { get; private set; }

        public bool Equals(UDTPropertyNetPropertyMap other)
        {
            return other != null &&
                UDTPropertyName == other.UDTPropertyName &&
                NetPropertyName == other.NetPropertyName;
        }
    }
}
