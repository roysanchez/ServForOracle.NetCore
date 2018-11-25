using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace ServForOracle.NetCore.Parameters
{
    public class PreparedParameter
    {
        public PreparedParameter(string construction, int startNumber, int lastNumber, IEnumerable<OracleParameter> parameters)
        {
            ConstructionString = construction;
            StartNumber = startNumber;
            LastNumber = lastNumber;
            Parameters = parameters;
        }

        public string ConstructionString { get; private set; }
        public int StartNumber { get; private set; }
        public int LastNumber { get; private set; }
        public IEnumerable<OracleParameter> Parameters { get; private set; }
    }
}
