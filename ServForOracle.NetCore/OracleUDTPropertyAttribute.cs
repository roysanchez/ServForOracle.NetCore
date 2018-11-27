using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OracleUDTPropertyAttribute: Attribute
    {
        public string PropertyName { get; set; }

        public OracleUDTPropertyAttribute(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            PropertyName = propertyName;
        }
    }
}
