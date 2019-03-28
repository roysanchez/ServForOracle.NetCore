using Microsoft.Extensions.Logging;
using Moq;
using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using ServForOracle.NetCore.UnitTests.TestTypes;
using ServForOracle.NetCore.Wrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ServiceForOracle_FunctionTests
    {
        public class TestClass : IEquatable<TestClass>
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }

            public bool Equals(TestClass other)
            {
                return other != null
                    && other.Prop1 == Prop1
                    && other.Prop2 == Prop2;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TestClass);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Prop1, Prop2);
            }
        }

        #region ExecuteFunctionAsync

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter returnParameter, string expectedValue)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{returnParameter.ParameterName} := {function}();"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(returnParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), returnParameter))
                .Returns(expectedValue);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, commonMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function);

            commandMock.Verify();
            Assert.Equal(expectedValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(returnParameter, oracleParameter);
        }

        #region ClrParam

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneClrParamInput(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputParam, string expectedValue)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(expectedValue);

            inputParam.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, commonMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function, inputParam.Object);

            commandMock.Verify();
            Assert.Equal(expectedValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneClrParamOutput(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> outputParam, string returnValue)
        {
            var type = typeof(string);
            var expectedValue = oracleParameter.Value;

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            outputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);
            outputParam.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, commonMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneInputOutputClrParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputOutputParam, string returnValue)
        {
            var type = typeof(string);
            var expectedValue = oracleParameter.Value;

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            inputOutputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.InputOutput);
            inputOutputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);
            inputOutputParam.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, commonMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function, inputOutputParam.Object);

            commandMock.Verify();
            inputOutputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneInputOneOutputClrParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputParam, Mock<ParamClrType<string>> outputParam, string returnValue)
        {
            var type = typeof(string);
            var expectedValue = oracleParameter.Value;

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1,:2);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            inputParam.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);

            outputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(i => i.GetOracleParameter(":2")).Returns(oracleParameter);
            outputParam.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
                .Verifiable();


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, commonMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function, inputParam.Object, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        #endregion ClrParam

        #region ObjectParam

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_ObjectReturn_OneInputOneOutputObjectParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter[] oracleParameters, Mock<ParamObject<TestClass>> inputParam, Mock<ParamObject<TestClass>> outputParam, TestClass returnValue, string declareRet, string declareP0, string declareP1, string constructor, string outputString, int lastNumber, Mock<MetadataOracleObject<TestClass>> metadataBaseMock, string retOutputQuery, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, new OracleParameter(), outputString);
            var type = typeof(string);
            //var expectedValue = oracleParameter.Value;

            //var message = $"declare{Environment.NewLine}{Environment.NewLine}"
            //    + $"begin{Environment.NewLine}{Environment.NewLine}"
            //    + $"{oracleParameter.ParameterName} := {function}(:1,:2);"
            //    + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareP1}{Environment.NewLine}" //p1
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + $"{constructor}{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(p0,p1);{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}"
                + $"{retOutputQuery}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            builderMock.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(It.IsAny<OracleUdtInfo>()))
                .Returns(metadataBaseMock.Object);
            metadataBaseMock.Setup(m => m.GetDeclareLine(typeof(TestClass), "ret", It.IsAny<OracleUdtInfo>()))
                .Returns(declareRet);
            metadataBaseMock.Setup(m => m.GetRefCursorQuery(It.IsAny<int>(), "ret"))
                .Returns(retOutputQuery);
            metadataBaseMock.Setup(m => m.GetValueFromRefCursor(typeof(TestClass), null))
                .Returns(returnValue);

            wrapperFactoryMock.Setup(w => w.Create(null))
                .Returns<OracleRefCursorWrapper>(null);


            //commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
            //    .Returns(oracleParameter);
            //commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), It.IsAny<OracleParameter>()))
            //    .Returns(returnValue);

            //inputParam.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            //inputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            inputParam.Setup(i => i.GetDeclareLine())
                .Returns(declareP0);
            inputParam.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputParam.Setup(i => i.GetOracleParameters(0))
                .Returns(oracleParameters);

            //outputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            //outputParam.Setup(i => i.GetOracleParameter(":2")).Returns(oracleParameter);
            //outputParam.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
            //    .Verifiable();

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP1);
            outputParam.Setup(o => o.PrepareOutputParameter(lastNumber))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, commonMock.Object);

            var result = await service.ExecuteFunctionAsync<TestClass>(function, inputParam.Object, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            //Assert.Equal(returnValue, result);
            //Assert.Equal(commandMock.Object.CommandText, message);
            //Assert.NotEmpty(commandMock.Object.Parameters);
            //AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        #endregion ObjectParam

        #endregion ExecuteFunctionAsync
    }
}
