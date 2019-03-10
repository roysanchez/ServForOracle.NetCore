using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class PreparedOutputParameterTests
    {
        [Theory, CustomAutoData]
        public void Constructor_TwoParameter(Param param, OracleParameter oracleParameter)
        {
            var prepared = new PreparedOutputParameter(param, oracleParameter);

            Assert.Equal(param, prepared.Parameter);
            Assert.Equal(oracleParameter, prepared.OracleParameter);
        }

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameter(Param param, OracleParameter oracleParameter, string outputString)
        {
            var prepared = new PreparedOutputParameter(param, oracleParameter, outputString);

            Assert.Equal(param, prepared.Parameter);
            Assert.Equal(oracleParameter, prepared.OracleParameter);
            Assert.Equal(outputString, prepared.OutputString);
        }
    }
}
