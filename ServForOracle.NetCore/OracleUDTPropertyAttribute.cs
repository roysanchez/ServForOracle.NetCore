using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OracleUdtPropertyAttributeAttribute: Attribute
    {
        public string PropertyName { get; set; }

        public OracleUdtPropertyAttributeAttribute(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            PropertyName = propertyName;
        }
    }
}
