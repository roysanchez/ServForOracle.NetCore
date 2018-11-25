using Oracle.ManagedDataAccess.Client;

namespace ServForOracle.NetCore.Parameters
{
    public class PreparedOutputParameter
    {
        public PreparedOutputParameter(Param parameter, OracleParameter oracleParameter, string refCursorString)
        {
            Parameter = parameter;
            OracleParameter = oracleParameter;
            RefCursorString = refCursorString;
        }

        public Param Parameter { get; private set; }
        public OracleParameter OracleParameter { get; private set; }
        public string RefCursorString { get; set; }
    }
}
