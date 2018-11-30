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
    public class OracleUDTAttribute : Attribute
    {
        /// <summary>
        /// The Oracle UDT object name
        /// </summary>
        public OracleUDTInfo UDTInfo { get; private set; }

        public OracleUDTAttribute(string objectName)
            :this(string.Empty, objectName)
        {
        }

        public OracleUDTAttribute(string schema, string objectName)
            :this(schema, objectName, string.Empty)
        {
        }

        public OracleUDTAttribute(string schema, string objectName, string collectionName)
            :this(schema, objectName, string.Empty, collectionName)
        {
        }

        public OracleUDTAttribute(string schema, string objectName, string collectionSchema, string collectionName)
        {
            UDTInfo = new OracleUDTInfo(schema, objectName, collectionName, collectionSchema);
        }
    }
}
