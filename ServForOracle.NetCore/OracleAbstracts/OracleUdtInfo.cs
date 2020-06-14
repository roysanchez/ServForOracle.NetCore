using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.OracleAbstracts
{
    public class OracleUdtInfo : IEquatable<OracleUdtInfo>, IEqualityComparer<OracleUdtInfo>
    {
        public string ObjectSchema { get; private set; }
        public string ObjectName { get; private set; }
        public string CollectionSchema { get; private set; }
        public string CollectionName { get; private set; }

        public OracleUdtInfo(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            if(objectName.Contains("|"))
            {
                var parts = objectName.Split('|');
                (ObjectSchema, ObjectName) = SplitAndValidateName(parts[0]);
                (CollectionSchema, CollectionName) = SplitAndValidateName(parts[1]);
            }
            else
            {
                (ObjectSchema, ObjectName) = SplitAndValidateName(objectName);
            }

            (string schema, string name) SplitAndValidateName(string name)
            {
                var errorString = $"The object {name} is invalid, it needs to have the format SCHEMA.OBJECT_NAME or" +
                " SCHEMA.OBJECTNAME|SCHEMA.COLLECTIONNAME";

                var objectParts = name.Split('.');
                if (objectParts.Length != 2)
                {
                    throw new ArgumentException(errorString, nameof(name));
                }

                if (string.IsNullOrWhiteSpace(objectParts[0]))
                {
                    throw new ArgumentException(errorString, nameof(name));
                }
                if (string.IsNullOrWhiteSpace(objectParts[1]))
                {
                    throw new ArgumentException(errorString, nameof(name));
                }

                return (objectParts[0], objectParts[1]);
            }
        }

        public OracleUdtInfo(string schema, string objectName)
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

        public OracleUdtInfo(string schema, string objectName, string collectionName)
            :this(schema, objectName)
        {
            if(string.IsNullOrWhiteSpace(collectionName))
            {
                throw new ArgumentNullException(nameof(collectionName));
            }

            CollectionName = collectionName.ToUpper();
            CollectionSchema = ObjectSchema;
        }

        public OracleUdtInfo(string objectSchema, string objectName, string collectionSchema, string collectionName)
            :this(objectSchema, objectName, collectionName)
        {
            if(string.IsNullOrWhiteSpace(collectionSchema))
            {
                throw new ArgumentNullException(nameof(collectionSchema));
            }

            CollectionSchema = collectionSchema.ToUpper();
        }

        public bool IsCollectionValid => !string.IsNullOrWhiteSpace(CollectionName);
        public string FullObjectName => $"{ObjectSchema}.{ObjectName}";

        public string FullCollectionName
        {
            get
            {
                if(string.IsNullOrWhiteSpace(CollectionSchema) || string.IsNullOrWhiteSpace(CollectionName))
                {
                    throw new ArgumentNullException("The UDT object is not set up correctly, doesn't have the collectionObject configured, udt=" + ToString());
                }
                return $"{CollectionSchema}.{CollectionName}";
            }
        }

        public override string ToString()
        {
            return $"objectSchema={ObjectSchema};objectName={ObjectName};collectionSchema={CollectionSchema};"
                + $"collectionName={CollectionName}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OracleUdtInfo);
        }

        public virtual bool Equals(OracleUdtInfo other)
        {
            return other != null &&
                   ObjectSchema == other.ObjectSchema &&
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

        public virtual bool Equals(OracleUdtInfo x, OracleUdtInfo y)
        {
            if (x is null && y is null)
            {
                return true;
            }
            else if(x is null || y is null)
            {
                return false;
            }
            else
            {
                return x.Equals(y);
            }
        }

        public int GetHashCode(OracleUdtInfo obj)
        {
            if (obj is null)
            {
                return 0;
            }
            else
            {
                return obj.GetHashCode();
            }
        }
    }
}
