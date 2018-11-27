using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using ServForOracle.NetCore.Parameters;
using System.Collections.Generic;

namespace ServForOracle.NetCore
{

    [OracleUDT("ramo_obj", "ramo_list", "uniserv")]
    public class RamoObj
    {
        public string CodRamo { get; set; }
        public string DescRamo { get; set; }
        public string TipoRamo { get; set; }
        //public string StsRamo { get; set; }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var serv = new ServForOracle(new OracleConnection(""));
            var ramon = new RamoObj() { CodRamo = "BABB" };
            var x = serv.ExecuteFunction<RamoObj>("uniserv.prueba_net_core_list_param",
                new ParamObject<RamoObj>(ramon, ParameterDirection.Input),
                new ParamCLRType<DateTime>(DateTime.Now, ParameterDirection.Input));

            Console.WriteLine("Hello World!");
        }

    }
}
