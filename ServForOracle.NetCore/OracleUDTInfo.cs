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

        public OracleUDTInfo(string objectName, string objectSchema = null, string collectionName = null,
            string collectionSchema = null)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            if (string.IsNullOrWhiteSpace(objectSchema))
            {
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
            else
            {
                ObjectName = objectName.ToUpper();
                ObjectSchema = objectSchema.ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                if (string.IsNullOrWhiteSpace(collectionSchema))
                {
                    var listParts = collectionName.ToUpper().Split('.');
                    if (listParts.Length > 1)
                    {
                        CollectionSchema = listParts[0];
                        CollectionName = listParts[1];
                    }
                    else
                    {
                        CollectionName = listParts[0];
                        CollectionSchema = ObjectSchema;
                    }
                }
                else
                {
                    CollectionName = collectionName.ToUpper();
                    CollectionSchema = collectionSchema.ToUpper();
                }
            }

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
            return HashCode.Combine(ObjectSchema, ObjectName, CollectionSchema, CollectionName);
        }
    }
}
