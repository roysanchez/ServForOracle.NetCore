using ServForOracle.NetCore.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServForOracle.NetCore
{
    public static class ProcedureAsyncExtensions
    {
        public static async Task ExecuteProcedureAsync<A>(this IServiceForOracle s, string procedure, A p1)
            => await s.ExecuteProcedureAsync(procedure, Param.Input(p1));
        public static async Task ExecuteProcedureAsync<A, B>(this IServiceForOracle s, string procedure, A p1, B p2)
            => await s.ExecuteProcedureAsync(procedure, Param.Input(p1), Param.Input(p2));
        public static async Task ExecuteProcedureAsync<A,B,C>(this IServiceForOracle s, string procedure, A p1, B p2, C p3)
            => await s.ExecuteProcedureAsync(procedure, Param.Input(p1), Param.Input(p2), Param.Input(p3));
        public static async Task ExecuteProcedureAsync<A,B,C,D>(this IServiceForOracle s, string procedure, A p1, B p2, C p3, D p4)
            => await s.ExecuteProcedureAsync(procedure, Param.Input(p1), Param.Input(p2), Param.Input(p3), Param.Input(p4));
    }
}
