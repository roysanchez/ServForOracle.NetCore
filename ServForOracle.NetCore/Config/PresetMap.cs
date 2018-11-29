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
            : this(new OracleUDTInfo(objectSchema, objectName, collectionSchema, collectionName), replacedProperties)
        {
        }

        public PresetMap(string schema, string objectName, string collectionName,
            params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(new OracleUDTInfo(schema, objectName, collectionName), replacedProperties)
        {
        }

        public PresetMap(string schema, string objectName, params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(new OracleUDTInfo(schema, objectName), replacedProperties)
        {
        }

        public PresetMap(string objectName, params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
            : this(new OracleUDTInfo(objectName), replacedProperties)
        {
        }

        public PresetMap(OracleUDTInfo info, params (Expression<Func<T, object>> property, string UDTPropertyName)[] replacedProperties)
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

        private UDTPropertyNetPropertyMap[] ConvertToUDTPropertyMapArray(
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

                    return new UDTPropertyNetPropertyMap(memberExpression.Member.Name, c.newName);
                }
            ).ToArray();
        }
    }

    public abstract class PresetMap
    {
        public Type Type { get; set; }
        public OracleUDTInfo Info { get; internal set; }
        internal UDTPropertyNetPropertyMap[] ReplacedProperties { get; set; }
    }
}
