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

        public OracleUDTAttribute(string objectName, string collectionName = null, string schema = null,
            string collectionSchema = null)
        {

            UDTInfo = new OracleUDTInfo(schema, objectName, collectionName, collectionSchema);
        }
    }
}
