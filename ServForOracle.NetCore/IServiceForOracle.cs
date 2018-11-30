using ServForOracle.NetCore.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServForOracle.NetCore
{
    public interface IServiceForOracle
    {
        void ExecuteProcedure(string procedure, params Param[] parameters);
        Task ExecuteProcedureAsync(string procedure, params Param[] parameters);
        T ExecuteFunction<T>(string function, params Param[] parameters);
        T ExecuteFunction<T>(string function, OracleUDTInfo udtInfo, params Param[] parameters);
        Task<T> ExecuteFunctionAsync<T>(string function, params Param[] parameters);
        Task<T> ExecuteFunctionAsync<T>(string function, OracleUDTInfo udtInfo, params Param[] parameters);
    }
}
