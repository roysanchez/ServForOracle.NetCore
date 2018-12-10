using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore
{
    public class OracleUdtInfo : IEquatable<OracleUdtInfo>, IEqualityComparer<OracleUdtInfo>
    {
        public OracleUdtInfo UnderType { get; private set; }
        public OracleUdtInfo OverType { get; private set; }
        public string ObjectSchema { get; private set; }
        public string ObjectName { get; private set; }

        public bool IsCollection { get; private set; }
        public bool IsCollectionValid => UnderType != null;

        public OracleUdtInfo(string objectName, bool isCollection)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            var errorString = $"The object {objectName} is invalid, it needs to havet the format SCHEMA.OBJECT_NAME";

            var objectParts = objectName.ToUpper().Split('.');
            if (objectParts.Length != 2)
            {
                throw new ArgumentException(nameof(objectName), errorString);
            }

            if (string.IsNullOrWhiteSpace(objectParts[0]))
            {
                throw new ArgumentException(nameof(objectName), errorString);
            }
            if (string.IsNullOrWhiteSpace(objectParts[1]))
            {
                throw new ArgumentException(nameof(objectName), errorString);
            }

            ObjectSchema = objectParts[0];
            ObjectName = objectParts[1];
            IsCollection = isCollection;
        }

        public OracleUdtInfo(string schema, string objectName, bool isCollection)
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
            IsCollection = isCollection;
        }

        public OracleUdtInfo(string schema, string collectionName, OracleUdtInfo underUdt)
            : this(schema, collectionName, isCollection: true)
        {
            UnderType = underUdt ?? throw new ArgumentNullException(nameof(underUdt));
            UnderType.OverType = this;
        }

        public string GetDeclaredLine(string parameterName)
        {
            if(IsCollection)
            {
                return $"{parameterName} {ObjectSchema} := {FullObjectName}();";
            }
            else
            {
                return $"{parameterName} {FullObjectName};";
            }
        }

        public string FullObjectName => $"{ObjectSchema}.{ObjectName}";

        public override string ToString()
        {
            return $"objectSchema={ObjectSchema};objectName={ObjectName};underType={UnderType?.ToString()};overType={OverType?.ToString()};";
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
                UnderType == other.UnderType &&
                OverType == other.OverType;
        }

        public override int GetHashCode()
        {
            var hashCode = -1158407366;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ObjectSchema);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ObjectName);

            if (UnderType != null)
            {
                hashCode = hashCode * -1521134295 + UnderType.GetHashCode();
            }
            if (OverType != null)
            {
                hashCode = hashCode * -1521134295 + OverType.GetHashCode();
            }


            return hashCode;
        }

        public virtual bool Equals(OracleUdtInfo x, OracleUdtInfo y)
        {
            if (x is null && y is null)
            {
                return true;
            }
            else if (x is null || y is null)
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
