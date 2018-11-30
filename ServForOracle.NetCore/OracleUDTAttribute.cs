using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore
{
    /// <summary>
    /// Attribute that specifies the Oracle UDT Name, must have the format "SCHEMA.UDTOBJECTNAME"
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OracleUdtAttributeAttributeAttribute : Attribute
    {
        /// <summary>
        /// The Oracle UDT object name
        /// </summary>
        public OracleUdtInfo UDTInfo { get; private set; }

        public OracleUdtAttributeAttributeAttribute(string objectName)
            :this(string.Empty, objectName)
        {
        }

        public OracleUdtAttributeAttributeAttribute(string schema, string objectName)
            :this(schema, objectName, string.Empty)
        {
        }

        public OracleUdtAttributeAttributeAttribute(string schema, string objectName, string collectionName)
            :this(schema, objectName, string.Empty, collectionName)
        {
        }

        public OracleUdtAttributeAttributeAttribute(string schema, string objectName, string collectionSchema, string collectionName)
        {
            UDTInfo = new OracleUdtInfo(schema, objectName, collectionSchema, collectionName);
        }
    }
}
