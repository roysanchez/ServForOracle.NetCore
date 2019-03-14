
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServForOracle.NetCore.Parameters
{
    public abstract class ParamManaged : Param
    {
        protected ParamManaged(Type type, object value, ParameterDirection direction)
    : base(type, value, direction)
        {
        }

        internal virtual string ParameterName { get; }
        internal abstract void SetParameterName(string name);
        internal abstract string GetDeclareLine();
        internal abstract PreparedOutputParameter PrepareOutputParameter(int startNumber);
    }
}
