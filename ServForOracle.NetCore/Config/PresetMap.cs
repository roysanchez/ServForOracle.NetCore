using ServForOracle.NetCore.Metadata;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ServForOracle.NetCore.Config
{
    public class PresetMap<T> : PresetMap
    {
        public PresetMap(string objectSchema, string objectName, string collectionSchema, string collectionName,
            params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(new OracleUdtInfo(collectionSchema, collectionName, new OracleUdtInfo(objectSchema, objectName,  isCollection: false)), replacedProperties)
        {
        }

        public PresetMap(string schema, string objectName, string collectionName,
            params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(new OracleUdtInfo(schema, collectionName,new OracleUdtInfo(schema, objectName, isCollection: false)), replacedProperties)
        {
        }

        public PresetMap(string schema, string objectName, params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(new OracleUdtInfo(schema, objectName), replacedProperties)
        {
        }

        public PresetMap(string objectName, params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(new OracleUdtInfo(objectName), replacedProperties)
        {
        }

        public PresetMap(OracleUdtInfo info, params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(replacedProperties)
        {
            Info = info;
        }

        private PresetMap(params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
        {
            if(replacedProperties == null)
            {
                throw new ArgumentNullException(nameof(replacedProperties));
            }

            ReplacedProperties = ConvertToUDTPropertyMapArray(replacedProperties);
        }

        private UdtPropertyNetPropertyMap[] ConvertToUDTPropertyMapArray(
                (Expression<Func<T, object>> action, string newName)[] replacedPropertiesUdtNames)
        {
            Type = typeof(T);
            return replacedPropertiesUdtNames.Select(
                c =>
                {
                    if (!(c.action.Body is MemberExpression memberExpression))
                    {
                        var unaryExpression = c.action.Body as UnaryExpression;
                        memberExpression = unaryExpression.Operand as MemberExpression;
                    }

                    return new UdtPropertyNetPropertyMap(memberExpression.Member.Name, c.newName);
                }
            ).ToArray();
        }
    }

    public abstract class PresetMap
    {
        public Type Type { get; set; }
        public OracleUdtInfo Info { get; internal set; }
        internal UdtPropertyNetPropertyMap[] ReplacedProperties { get; set; }
    }
}
