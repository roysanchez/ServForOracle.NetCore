using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServForOracle.NetCore.Metadata
{
    internal class MetadataOracleBoolean : MetadataOracle
    {
        public string GetDeclareLine(string parameterName)
        {
            return $"{parameterName} boolean;";
        }

        public string OutputString(int startNumber, string fieldName)
        {
            var retStr = $@"
                if({fieldName}) then
                    :{startNumber} := 1;
                else
                    :{startNumber} := 0;
                end if;";

            return retStr;
        }

        public OracleParameter GetOracleParameter(bool? value, int counter)
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

        public object GetBooleanValue(object value)
        {
            if (value.Equals(0))
            {
                return false;
            }
            else if (value.Equals(1))
            {
                return true;
            }
            else
            {
                return null;
            }
        }
    }
}
