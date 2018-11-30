using Oracle.ManagedDataAccess.Client;
using System;
using Xunit;
using ServForOracle.NetCore.Config;

namespace ServForOracle.NetCore.UnitTests
{
    public class UnitTest1
    {
        public class TestClass
        {
            public string Roy { get; set; }
        }
        [Fact]
        public void Test1()
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var test = new PresetMap<TestClass>(new OracleUdtInfo("uniserv.Roy"), (c => c.Roy, "Roy2"));

            //var serv = new ServForOracle(new OracleConnection(""));
            //var roy = "1234";
            //serv.ExecuteProcedure("uniserv.proc_prueba_net_core_out_1", roy);

            //PresetMappings.AddOracleUDTConfiguration<RamoObj>(new OracleUDTInfo("ramo_obj", "uniserv", "ramo_list"),
            //    (c => c.TipoRamo2, "tiporamo"));
            //var serv = new ServForOracle(new OracleConnection("Data Source=172.16.99.111:1522/PX04;User Id=uniserv;Password=uniserv;"));
            //var ramon = new RamoObj() { CodRamo = "BABB" };
            //var p = Param.InputOutput(new RamoObj[] { ramon }); //new ParamObject<RamoObj[]>(new RamoObj[] { ramon }, ParameterDirection.InputOutput);
            //var dato2 = Param.Output<string>(); // new ParamCLRType<string>(null, ParameterDirection.Output);
            //var dato5 = Param.Output<string>();//new ParamCLRType<string>("C", ParameterDirection.Output);
            //serv.ExecuteProcedure("uniserv.prueba_net_core_out",
            //    Param.Input("A"),
            //    dato2,
            //    Param.Input("B"),
            //    p,
            //    Param.Input("C"),
            //    dato5,
            //    Param.Input("D")
            //    //new ParamObject<RamoObj>(ramon, ParameterDirection.Input, new OracleUDTInfo("RAMO_OBJ", "UNISERV")),

            //    //new Param<DateTime>(DateTime.Now, ParameterDirection.Input)
        }
    }
}
