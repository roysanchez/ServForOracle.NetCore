using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServForOracle.NetCore
{
    public interface IServiceForOracle
    {
        void ExecuteProcedure(string procedure, params IParam[] parameters);
        Task ExecuteProcedureAsync(string procedure, params IParam[] parameters);
        T ExecuteFunction<T>(string function, params IParam[] parameters);
        T ExecuteFunction<T>(string function, OracleUdtInfo udtInfo, params IParam[] parameters);
        Task<T> ExecuteFunctionAsync<T>(string function, params IParam[] parameters);
        Task<T> ExecuteFunctionAsync<T>(string function, OracleUdtInfo udtInfo, params IParam[] parameters);
    }
}
