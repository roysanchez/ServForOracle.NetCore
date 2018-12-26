using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.OracleAbstracts
{
    /// <summary>
    /// Attribute that specifies the Oracle UDT Name, must have the format "SCHEMA.UDTOBJECTNAME"
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OracleUdtAttribute : Attribute
    {
        /// <summary>
        /// The Oracle UDT object name
        /// </summary>
        public OracleUdtInfo UDTInfo { get; private set; }

        public OracleUdtAttribute(string objectName)
        {
            UDTInfo = new OracleUdtInfo(objectName);
        }

        public OracleUdtAttribute(string schema, string objectName)
        {
            UDTInfo = new OracleUdtInfo(schema, objectName);
        }

        public OracleUdtAttribute(string schema, string objectName, string collectionName)
        {
            UDTInfo = new OracleUdtInfo(schema, objectName, collectionName);
        }

        public OracleUdtAttribute(string schema, string objectName, string collectionSchema, string collectionName)
        {
            UDTInfo = new OracleUdtInfo(schema, objectName, collectionSchema, collectionName);
        }
    }
}
