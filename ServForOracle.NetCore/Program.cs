using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using ServForOracle.NetCore.Parameters;

namespace ServForOracle.NetCore
{

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
            var con = new OracleConnection("");
            con.Open();

            var serv = new ServForOracle();
            var ramon = new RamoObj() { CodRamo = "BABB" };
            var x = serv.ExecuteFunction<RamoObj>("uniserv.prueba_net_core_list_param", "UNISERV", "RAMO_OBJ", "RAMO_LIST", con,
                new ParamObject<RamoObj>(ramon, "UNISERV", "RAMO_OBJ", con, ParameterDirection.Input),
                new Param<DateTime>(DateTime.Now, ParameterDirection.Input));

            Console.WriteLine("Hello World!");
        }
    }
}
