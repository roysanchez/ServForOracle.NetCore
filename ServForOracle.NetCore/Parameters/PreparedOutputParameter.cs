using Oracle.ManagedDataAccess.Client;

namespace ServForOracle.NetCore.Parameters
{
    public class PreparedOutputParameter
    {
        public PreparedOutputParameter(Param parameter, OracleParameter oracleParameter)
        {
            Parameter = parameter;
            OracleParameter = oracleParameter;
        }
        public PreparedOutputParameter(Param parameter, OracleParameter oracleParameter, string outputString)
            :this(parameter, oracleParameter)
        {
            OutputString = outputString;
        }

        public Param Parameter { get; private set; }
        public OracleParameter OracleParameter { get; private set; }
        public string OutputString { get; set; }
    }
}
