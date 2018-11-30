using ServForOracle.NetCore.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore
{
    public static class FunctionExtensions
    {
        public static T ExecuteFunction<T, A>(this IServiceForOracle s, string function, ref A p1)
        {
            var _p1 = Param.InputOutput(p1);

            var temp = s.ExecuteFunction<T>(function, _p1);

            p1 = (A)_p1.Value;
            return temp;
        }

        public static T ExecuteFunction<T, A, B>(this IServiceForOracle s, string procedure, ref A p1, ref B p2)
        {
            var _p1 = Param.InputOutput(p1);
            var _p2 = Param.InputOutput(p2);

            var temp = s.ExecuteFunction<T>(procedure, _p1, _p2);

            p1 = (A)_p1.Value;
            p2 = (B)_p2.Value;

            return temp;
        }

        public static T ExecuteFunction<T, A, B, C>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, ref C p3)
        {
            var _p1 = Param.InputOutput(p1);
            var _p2 = Param.InputOutput(p2);
            var _p3 = Param.InputOutput(p3);

            var temp = s.ExecuteFunction<T>(procedure, _p1, _p2, _p3);

            p1 = (A)_p1.Value;
            p2 = (B)_p2.Value;
            p3 = (C)_p3.Value;

            return temp;
        }

        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, ref C p3, ref D p4)
        {
            var _p1 = Param.InputOutput(p1);
            var _p2 = Param.InputOutput(p2);
            var _p3 = Param.InputOutput(p3);
            var _p4 = Param.InputOutput(p4);

            var temp = s.ExecuteFunction<T>(procedure, _p1, _p2, _p3, _p4);

            p1 = (A)_p1.Value;
            p2 = (B)_p2.Value;
            p3 = (C)_p3.Value;
            p4 = (D)_p4.Value;

            return temp;
        }

        public static T ExecuteFunction<T, A>(this IServiceForOracle s, string procedure, A p1)
        => ExecuteFunction<T, A>(s, procedure, ref p1);
        
        public static T ExecuteFunction<T, A, B>(this IServiceForOracle s, string procedure, A p1, B p2)
         => s.ExecuteFunction<T, A, B>(procedure, ref p1, ref p2);
        public static T ExecuteFunction<T, A, B, C>(this IServiceForOracle s, string procedure, A p1, B p2, C p3)
         => s.ExecuteFunction<T, A, B, C>(procedure, ref p1, ref p2, ref p3);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);


        public static T ExecuteFunction<T, A, B>(this IServiceForOracle s, string procedure, A p1, ref B p2)
         => s.ExecuteFunction<T, A, B>(procedure, ref p1, ref p2);

        public static T ExecuteFunction<T, A, B, C>(this IServiceForOracle s, string procedure, A p1, ref B p2, C p3)
         => s.ExecuteFunction<T, A, B, C>(procedure, ref p1, ref p2, ref p3);
        public static T ExecuteFunction<T, A, B, C>(this IServiceForOracle s, string procedure, A p1, B p2, ref C p3)
         => s.ExecuteFunction<T, A, B, C>(procedure, ref p1, ref p2, ref p3);
        public static T ExecuteFunction<T, A, B, C>(this IServiceForOracle s, string procedure, A p1, ref B p2, ref C p3)
         => s.ExecuteFunction<T, A, B, C>(procedure, ref p1, ref p2, ref p3);

        //4
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, ref C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, ref B p2, C p3, ref D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);


        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, ref C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, ref C p3, ref D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);

        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, ref A p1, B p2, C p3, ref D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);


        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, ref C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, C p3, ref D p4)
        => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, ref B p2, ref C p3, ref D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);


        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, ref C p3, D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, ref C p3, ref D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);


        public static T ExecuteFunction<T, A, B, C, D>(this IServiceForOracle s, string procedure, A p1, B p2, C p3, ref D p4)
         => s.ExecuteFunction<T, A, B, C, D>(procedure, ref p1, ref p2, ref p3, ref p4);
    }
}
