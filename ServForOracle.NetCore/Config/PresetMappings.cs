using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ServForOracle.NetCore.Config
{
    public class PresetMappings
    {
        public void AddOracleUDT<T>(OracleUDTInfo info,
            params (Expression<Func<T, object>> action, string propertyName)[] propiedades)
        {
            AddOracleUDTConfiguration(info, propiedades);
        }

        public static void AddOracleUDTConfiguration<T>(OracleUDTInfo info,
            params (Expression<Func<T, object>> action, string propertyName)[] propiedades)
        {
            MetadataBuilder.AddOracleUDTPresets(typeof(T), info, ConvertToUDTPropertyMapArray(propiedades));
        }

        private static UDTPropertyNetPropertyMap[] ConvertToUDTPropertyMapArray<T>(
                (Expression<Func<T, object>> action, string newName)[] replacedPropertiesUdtNames)
        {
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
}
