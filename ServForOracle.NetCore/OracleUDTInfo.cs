using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore
{
    public class OracleUDTInfo : IEquatable<OracleUDTInfo>
    {
        public string ObjectSchema { get; private set; }
        public string ObjectName { get; private set; }
        public string CollectionSchema { get; private set; }
        public string CollectionName { get; private set; }

        public OracleUDTInfo(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            var objectParts = objectName.ToUpper().Split('.');
            if (objectParts.Length != 2)
            {
                throw new ArgumentException("The object name is invalid");
            }

            if (string.IsNullOrWhiteSpace(objectParts[0]))
            {
                throw new ArgumentNullException("The object schema is invalid");
            }
            if (string.IsNullOrWhiteSpace(objectParts[1]))
            {
                throw new ArgumentNullException("the object name is invalid");
            }
            ObjectSchema = objectParts[0];
            ObjectName = objectParts[1];

        }

        public OracleUDTInfo(string schema, string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (string.IsNullOrWhiteSpace(schema))
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            ObjectName = objectName.ToUpper();
            ObjectSchema = schema.ToUpper();
        }

        public OracleUDTInfo(string schema, string objectName, string collectionName)
            :this(schema, objectName)
        {
            if(string.IsNullOrWhiteSpace(collectionName))
            {
                throw new ArgumentNullException(nameof(collectionName));
            }

            CollectionName = collectionName.ToUpper();
            CollectionSchema = ObjectSchema;
        }

        public OracleUDTInfo(string objectSchema, string objectName, string collectionSchema, string collectionName)
            :this(objectSchema, objectName, collectionName)
        {
            if(string.IsNullOrWhiteSpace(collectionSchema))
            {
                throw new ArgumentNullException(nameof(collectionSchema));
            }

            CollectionSchema = collectionSchema;
        }

        public bool IsCollectionValid => !string.IsNullOrWhiteSpace(CollectionName);
        public string FullObjectName => $"{ObjectSchema}.{ObjectName}";
        public string FullCollectionName => $"{CollectionSchema}.{CollectionName}";

        public override bool Equals(object obj)
        {
            return Equals(obj as OracleUDTInfo);
        }

        public bool Equals(OracleUDTInfo other)
        {
            return ObjectSchema == other.ObjectSchema &&
                   ObjectName == other.ObjectName &&
                   CollectionSchema == other.CollectionSchema &&
                   CollectionName == other.CollectionName;
        }
        public override int GetHashCode()
        {
            var hashCode = -1158407366;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ObjectSchema);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ObjectName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CollectionSchema);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CollectionName);
            return hashCode;
        }
    }
}
