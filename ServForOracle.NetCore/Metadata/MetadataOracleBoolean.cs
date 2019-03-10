using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleBoolean : MetadataBase
    {
        public virtual string GetDeclareLine(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentNullException(nameof(parameterName));

            return $"{parameterName} boolean;";
        }

        public virtual string OutputString(int startNumber, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            var retStr = $@"
                if({fieldName}) then
                    :{startNumber} := 1;
                else
                    :{startNumber} := 0;
                end if;";

            return retStr;
        }

        public virtual OracleParameter GetOracleParameter(bool? value, int counter)
        {
            byte? tempValue = null;
            if (value.HasValue)
            {
                tempValue = value.Value ? (byte)1 : (byte)0;
            }

            return new OracleParameter($":{counter}", tempValue)
            {
                Direction = ParameterDirection.Output
            };
        }

        public virtual object GetBooleanValue(object value)
        {
            if (value is OracleDecimal v)
            {
                return v.ToByte() == 1;
            }
            else
            {
                return null;
            }
        }
    }
}
