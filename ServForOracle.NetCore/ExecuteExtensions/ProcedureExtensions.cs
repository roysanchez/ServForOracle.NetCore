using ServForOracle.NetCore.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore
{
    public static class ProcedureExtensions
    {
        public static void ExecuteProcedure<A>(this IServiceForOracle s, string procedure,
            ref A p1)
        {
            var _p1 = Param.InputOutput(p1);

            s.ExecuteProcedure(procedure, _p1);

            p1 = _p1.Value;
        }

        public static void ExecuteProcedure<A, B>(this IServiceForOracle s, string procedure, ref A p1, ref B p2)
        {
            var _p1 = Param.InputOutput(p1);
            var _p2 = Param.InputOutput(p2);

            s.ExecuteProcedure(procedure, _p1, _p2);

            p1 = _p1.Value;
            p2 = _p2.Value;
        }

        public static void ExecuteProcedure<A, B, C>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, ref C p3)
        {
            var _p1 = Param.InputOutput(p1);
            var _p2 = Param.InputOutput(p2);
            var _p3 = Param.InputOutput(p3);

            s.ExecuteProcedure(procedure, _p1, _p2, _p3);

            p1 = _p1.Value;
            p2 = _p2.Value;
            p3 = _p3.Value;
        }

        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, ref C p3, ref D p4)
        {
            var _p1 = Param.InputOutput(p1);
            var _p2 = Param.InputOutput(p2);
            var _p3 = Param.InputOutput(p3);
            var _p4 = Param.InputOutput(p4);

            s.ExecuteProcedure(procedure, _p1, _p2, _p3, _p4);

            p1 = _p1.Value;
            p2 = _p2.Value;
            p3 = _p3.Value;
            p4 = _p4.Value;
        }
        
        public static void ExecuteProcedure<A>(this IServiceForOracle s, string procedure, A p1)
         => s.ExecuteProcedure(procedure, ref p1);
        public static void ExecuteProcedure<A, B>(this IServiceForOracle s, string procedure, A p1, B p2)
         => s.ExecuteProcedure(procedure, ref p1, ref p2);
        public static void ExecuteProcedure<A, B, C>(this IServiceForOracle s, string procedure, A p1, B p2, C p3)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);

        public static void ExecuteProcedure<A, B>(this IServiceForOracle s, string procedure, A p1, ref B p2)
         => s.ExecuteProcedure(procedure, ref p1, ref p2);

        public static void ExecuteProcedure<A, B, C>(this IServiceForOracle s, string procedure, A p1, ref B p2, C p3)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3);
        public static void ExecuteProcedure<A, B, C>(this IServiceForOracle s, string procedure, A p1, B p2, ref C p3)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3);
        public static void ExecuteProcedure<A, B, C>(this IServiceForOracle s, string procedure, A p1, ref B p2, ref C p3)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3);

        //4
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, ref C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, C p3, ref D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        

        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, ref C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, ref C p3, ref D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);

        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, C p3, ref D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        

        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, ref C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, C p3, ref D p4)
        => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, ref C p3, ref D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        

        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, ref C p3, D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, ref C p3, ref D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);


        public static void ExecuteProcedure<A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, C p3, ref D p4)
         => s.ExecuteProcedure(procedure, ref p1, ref p2, ref p3, ref p4);
    }
}
