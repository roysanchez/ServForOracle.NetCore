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
        internal async Task ExecuteFunctionAsync_StringReturn_OpensClosedConnection(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter returnParameter, string expectedValue, Mock<IMetadataFactory> metadataFactoryMock)
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
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Closed);
            connectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(returnParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), returnParameter))
                .Returns(expectedValue);
            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function);

            commandMock.Verify();
            Assert.Equal(expectedValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(returnParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter returnParameter, string expectedValue, Mock<IMetadataFactory> metadataFactoryMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(returnParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), returnParameter))
                .Returns(expectedValue);
            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

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
        internal async Task ExecuteFunctionAsync_StringReturn_OneClrParamInput(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputParam, string expectedValue, Mock<IMetadataFactory> metadataFactoryMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(expectedValue);

            inputParam.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function, inputParam.Object);

            commandMock.Verify();
            Assert.Equal(expectedValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneClrParamOutput(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> outputParam, string returnValue, Mock<IMetadataFactory> metadataFactoryMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            outputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);
            outputParam.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
                .Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneInputOutputClrParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputOutputParam, string returnValue, Mock<IMetadataFactory> metadataFactoryMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            inputOutputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.InputOutput);
            inputOutputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);
            inputOutputParam.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
                .Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<string>(function, inputOutputParam.Object);

            commandMock.Verify();
            inputOutputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneInputOneOutputClrParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputParam, Mock<ParamClrType<string>> outputParam, string returnValue, Mock<IMetadataFactory> metadataFactoryMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

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
        internal async Task ExecuteFunctionAsync_ObjectReturn_OneInputObjectParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter[] oracleParameters, Mock<ParamObject<TestClass>> inputParam, TestClass returnValue, string declareRet, string declareP0, string constructor, int lastNumber, Mock<MetadataOracleObject<TestClass>> metadataBaseMock, string retOutputQuery, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + $"{constructor}{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(p0);{Environment.NewLine}{Environment.NewLine}"
                + $"{retOutputQuery}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            inputParam.Setup(i => i.GetDeclareLine())
                .Returns(declareP0);
            inputParam.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputParam.Setup(i => i.GetOracleParameters(0))
                .Returns(oracleParameters);

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<TestClass>(function, inputParam.Object);

            commandMock.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(oracleParameters.Length + 1, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_ObjectReturn_OneOutputObjectParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, Mock<ParamObject<TestClass>> outputParam, TestClass returnValue, string declareRet, string declareP0, string outputString, Mock<MetadataOracleObject<TestClass>> metadataBaseMock, string retOutputQuery, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, new OracleParameter(), outputString);
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(p0);{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}"
                + $"{retOutputQuery}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP0);
            outputParam.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<TestClass>(function, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(2, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_ObjectReturn_OneInputOneOutputObjectParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter[] oracleParameters, Mock<ParamObject<TestClass>> inputParam, Mock<ParamObject<TestClass>> outputParam, TestClass returnValue, string declareRet, string declareP0, string declareP1, string constructor, string outputString, int lastNumber, Mock<MetadataOracleObject<TestClass>> metadataBaseMock, string retOutputQuery, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, new OracleParameter(), outputString);
            var type = typeof(string);

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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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
            
            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            inputParam.Setup(i => i.GetDeclareLine())
                .Returns(declareP0);
            inputParam.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputParam.Setup(i => i.GetOracleParameters(0))
                .Returns(oracleParameters);

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP1);
            outputParam.Setup(o => o.PrepareOutputParameter(lastNumber))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<TestClass>(function, inputParam.Object, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(oracleParameters.Length + 2, commandMock.Object.Parameters.Count);
        }

        #endregion ObjectParam

        #region BooleanParam

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_BooleanReturn_OneInputBooleanParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, OracleParameter oracleParameter, Mock<ParamBoolean> inputParam, bool returnValue, string declareRet, Mock<MetadataOracleBoolean> metadataBaseMock, string retOutputString, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(:0);{Environment.NewLine}{Environment.NewLine}"
                + $"{retOutputString}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            metadataBaseMock.Setup(m => m.GetDeclareLine("ret")).Returns(declareRet);
            metadataBaseMock.Setup(m => m.OutputString(1, "ret")).Returns(retOutputString);
            metadataBaseMock.Setup(m => m.GetBooleanValue(null)).Returns(returnValue);

            metadataFactoryMock.Setup(m => m.CreateBoolean()).Returns(metadataBaseMock.Object);


            wrapperFactoryMock.Setup(w => w.Create(null))
                .Returns<OracleRefCursorWrapper>(null);

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.SetParameterName(":0"));

            inputParam.Setup(i => i.GetOracleParameter(0)).Returns(oracleParameter);


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<bool>(function, inputParam.Object);

            commandMock.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(2, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_BooleanReturn_OneOutputBooleanParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, OracleParameter oracleParameter, Mock<ParamBoolean> outputParam, bool returnValue, string declareRet, string declareP0, string outputString, Mock<MetadataOracleBoolean> metadataBaseMock, string retOutputString, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock, string outputParameterName)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, oracleParameter, outputString);
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}({outputParameterName});{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}"
                + $"{retOutputString}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            metadataBaseMock.Setup(m => m.GetDeclareLine("ret")).Returns(declareRet);
            metadataBaseMock.Setup(m => m.OutputString(1, "ret")).Returns(retOutputString);
            metadataBaseMock.Setup(m => m.GetBooleanValue(null)).Returns(returnValue);

            metadataFactoryMock.Setup(m => m.CreateBoolean()).Returns(metadataBaseMock.Object);


            wrapperFactoryMock.Setup(w => w.Create(null))
                .Returns<OracleRefCursorWrapper>(null);

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);

            outputParam.Setup(o => o.SetParameterName("p0"));
            outputParam.SetupGet(o => o.ParameterName).Returns(outputParameterName);
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP0);
            outputParam.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<bool>(function, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(2, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_BooleanReturn_OneInputOneOutputBooleanParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, OracleParameter oracleParameter, Mock<ParamBoolean> inputParam, Mock<ParamBoolean> outputParam, bool returnValue, string declareRet, string declareP0, string outputString, Mock<MetadataOracleBoolean> metadataBaseMock, string retOutputString, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock, string outputParameterName)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, oracleParameter, outputString);
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(:0,{outputParameterName});{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}"
                + $"{retOutputString}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            metadataBaseMock.Setup(m => m.GetDeclareLine("ret")).Returns(declareRet);
            metadataBaseMock.Setup(m => m.OutputString(2, "ret")).Returns(retOutputString);
            metadataBaseMock.Setup(m => m.GetBooleanValue(null)).Returns(returnValue);

            metadataFactoryMock.Setup(m => m.CreateBoolean()).Returns(metadataBaseMock.Object);


            wrapperFactoryMock.Setup(w => w.Create(null))
                .Returns<OracleRefCursorWrapper>(null);

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.SetParameterName(":0"));
            
            inputParam.Setup(i => i.GetOracleParameter(0)).Returns(oracleParameter);

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            
            outputParam.Setup(o => o.SetParameterName("p0"));
            outputParam.SetupGet(o => o.ParameterName).Returns(outputParameterName);
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP0);
            outputParam.Setup(o => o.PrepareOutputParameter(1))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = await service.ExecuteFunctionAsync<bool>(function, inputParam.Object, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(3, commandMock.Object.Parameters.Count);
        }

        #endregion BooleanParam

        #endregion ExecuteFunctionAsync

        #region ExecuteFunction

        [Theory, CustomAutoData]
        internal void ExecuteFunction_StringReturn_OpensClosedConnection(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter returnParameter, string expectedValue, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{returnParameter.ParameterName} := {function}();"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Closed);
            connectionMock.Setup(m => m.Open());

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(returnParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), returnParameter))
                .Returns(expectedValue);
            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<string>(function);

            commandMock.Verify();
            Assert.Equal(expectedValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(returnParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_StringReturn(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter returnParameter, string expectedValue, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{returnParameter.ParameterName} := {function}();"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(returnParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), returnParameter))
                .Returns(expectedValue);
            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<string>(function);

            commandMock.Verify();
            Assert.Equal(expectedValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(returnParameter, oracleParameter);
        }

        #region ClrParam

        [Theory, CustomAutoData]
        internal void ExecuteFunction_StringReturn_OneClrParamInput(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputParam, string expectedValue, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(expectedValue);

            inputParam.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<string>(function, inputParam.Object);

            commandMock.Verify();
            Assert.Equal(expectedValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_StringReturn_OneClrParamOutput(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> outputParam, string returnValue, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);
            var expectedValue = oracleParameter.Value;

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);
            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            outputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);
            outputParam.Setup(o => o.SetOutputValue(expectedValue))
                .Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<string>(function, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_StringReturn_OneInputOutputClrParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputOutputParam, string returnValue, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);
            var expectedValue = oracleParameter.Value;

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            inputOutputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.InputOutput);
            inputOutputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);
            inputOutputParam.Setup(o => o.SetOutputValue(expectedValue))
                .Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<string>(function, inputOutputParam.Object);

            commandMock.Verify();
            inputOutputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.All(commandMock.Object.Parameters, c => Assert.Same(oracleParameter, c));
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_StringReturn_OneInputOneOutputClrParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputParam, Mock<ParamClrType<string>> outputParam, string returnValue, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);
            var expectedValue = oracleParameter.Value;

            var message = $"declare{Environment.NewLine}{Environment.NewLine}"
                + $"begin{Environment.NewLine}{Environment.NewLine}"
                + $"{oracleParameter.ParameterName} := {function}(:1,:2);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            commonMock.Setup(c => c.GetOracleParameter(type, ParameterDirection.Output, ":0", DBNull.Value))
                .Returns(oracleParameter);
            commonMock.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleParameter))
                .Returns(returnValue);

            inputParam.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.GetOracleParameter(":1")).Returns(oracleParameter);

            outputParam.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(i => i.GetOracleParameter(":2")).Returns(oracleParameter);
            outputParam.Setup(o => o.SetOutputValue(expectedValue)).Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<string>(function, inputParam.Object, outputParam.Object);

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
        internal void ExecuteFunction_ObjectReturn_OneInputObjectParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter[] oracleParameters, Mock<ParamObject<TestClass>> inputParam, TestClass returnValue, string declareRet, string declareP0, string constructor, int lastNumber, Mock<MetadataOracleObject<TestClass>> metadataBaseMock, string retOutputQuery, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + $"{constructor}{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(p0);{Environment.NewLine}{Environment.NewLine}"
                + $"{retOutputQuery}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.LoadObjectMetadata(builderMock.Object));
            inputParam.Setup(i => i.GetDeclareLine())
                .Returns(declareP0);
            inputParam.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputParam.Setup(i => i.GetOracleParameters(0))
                .Returns(oracleParameters);

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<TestClass>(function, inputParam.Object);

            commandMock.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(oracleParameters.Length + 1, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_ObjectReturn_OneOutputObjectParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, Mock<ParamObject<TestClass>> outputParam, TestClass returnValue, string declareRet, string declareP0, string outputString, Mock<MetadataOracleObject<TestClass>> metadataBaseMock, string retOutputQuery, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, new OracleParameter(), outputString);
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(p0);{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}"
                + $"{retOutputQuery}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(o => o.LoadObjectMetadata(builderMock.Object));
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP0);
            outputParam.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValue(null))
                .Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<TestClass>(function, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(2, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_ObjectReturn_OneInputOneOutputObjectParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter[] oracleParameters, Mock<ParamObject<TestClass>> inputParam, Mock<ParamObject<TestClass>> outputParam, TestClass returnValue, string declareRet, string declareP0, string declareP1, string constructor, string outputString, int lastNumber, Mock<MetadataOracleObject<TestClass>> metadataBaseMock, string retOutputQuery, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, new OracleParameter(), outputString);
            var type = typeof(string);

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

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.LoadObjectMetadata(builderMock.Object));
            inputParam.Setup(i => i.GetDeclareLine())
                .Returns(declareP0);
            inputParam.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputParam.Setup(i => i.GetOracleParameters(0))
                .Returns(oracleParameters);

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputParam.Setup(o => o.LoadObjectMetadata(builderMock.Object));
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP1);
            outputParam.Setup(o => o.PrepareOutputParameter(lastNumber))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValue(null)).Verifiable();

            metadataFactoryMock.Setup(m => m.CreateCommon()).Returns(commonMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<TestClass>(function, inputParam.Object, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(oracleParameters.Length + 2, commandMock.Object.Parameters.Count);
        }

        #endregion ObjectParam

        #region BooleanParam

        [Theory, CustomAutoData]
        internal void ExecuteFunction_BooleanReturn_OneInputBooleanParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, OracleParameter oracleParameter, Mock<ParamBoolean> inputParam, bool returnValue, string declareRet, Mock<MetadataOracleBoolean> metadataBaseMock, string retOutputString, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock)
        {
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(:0);{Environment.NewLine}{Environment.NewLine}"
                + $"{retOutputString}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            metadataBaseMock.Setup(m => m.GetDeclareLine("ret")).Returns(declareRet);
            metadataBaseMock.Setup(m => m.OutputString(1, "ret")).Returns(retOutputString);
            metadataBaseMock.Setup(m => m.GetBooleanValue(null)).Returns(returnValue);

            metadataFactoryMock.Setup(m => m.CreateBoolean()).Returns(metadataBaseMock.Object);


            wrapperFactoryMock.Setup(w => w.Create(null))
                .Returns<OracleRefCursorWrapper>(null);

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.SetParameterName(":0"));

            inputParam.Setup(i => i.GetOracleParameter(0)).Returns(oracleParameter);


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<bool>(function, inputParam.Object);

            commandMock.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(2, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_BooleanReturn_OneOutputBooleanParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, OracleParameter oracleParameter, Mock<ParamBoolean> outputParam, bool returnValue, string declareRet, string declareP0, string outputString, Mock<MetadataOracleBoolean> metadataBaseMock, string retOutputString, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock, string outputParameterName)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, oracleParameter, outputString);
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}({outputParameterName});{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}"
                + $"{retOutputString}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            metadataBaseMock.Setup(m => m.GetDeclareLine("ret")).Returns(declareRet);
            metadataBaseMock.Setup(m => m.OutputString(1, "ret")).Returns(retOutputString);
            metadataBaseMock.Setup(m => m.GetBooleanValue(null)).Returns(returnValue);

            metadataFactoryMock.Setup(m => m.CreateBoolean()).Returns(metadataBaseMock.Object);


            wrapperFactoryMock.Setup(w => w.Create(null))
                .Returns<OracleRefCursorWrapper>(null);

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);

            outputParam.Setup(o => o.SetParameterName("p0"));
            outputParam.SetupGet(o => o.ParameterName).Returns(outputParameterName);
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP0);
            outputParam.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValue(null)).Verifiable();


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<bool>(function, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(2, commandMock.Object.Parameters.Count);
        }

        [Theory, CustomAutoData]
        internal void ExecuteFunction_BooleanReturn_OneInputOneOutputBooleanParam(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, OracleParameter oracleParameter, Mock<ParamBoolean> inputParam, Mock<ParamBoolean> outputParam, bool returnValue, string declareRet, string declareP0, string outputString, Mock<MetadataOracleBoolean> metadataBaseMock, string retOutputString, Mock<IOracleRefCursorWrapperFactory> wrapperFactoryMock, Mock<IMetadataFactory> metadataFactoryMock, string outputParameterName)
        {
            var prepared = new PreparedOutputParameter(outputParam.Object, oracleParameter, outputString);
            var type = typeof(string);

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareRet}{Environment.NewLine}" //ret
                + $"{Environment.NewLine}begin{Environment.NewLine}{Environment.NewLine}"
                + $"ret := {function}(:0,{outputParameterName});{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}"
                + $"{retOutputString}{Environment.NewLine}"
                + $"{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQuery())
                .Returns(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            metadataBaseMock.Setup(m => m.GetDeclareLine("ret")).Returns(declareRet);
            metadataBaseMock.Setup(m => m.OutputString(2, "ret")).Returns(retOutputString);
            metadataBaseMock.Setup(m => m.GetBooleanValue(null)).Returns(returnValue);

            metadataFactoryMock.Setup(m => m.CreateBoolean()).Returns(metadataBaseMock.Object);


            wrapperFactoryMock.Setup(w => w.Create(null))
                .Returns<OracleRefCursorWrapper>(null);

            inputParam.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputParam.Setup(i => i.SetParameterName(":0"));

            inputParam.Setup(i => i.GetOracleParameter(0)).Returns(oracleParameter);

            outputParam.Setup(o => o.Direction).Returns(ParameterDirection.Output);

            outputParam.Setup(o => o.SetParameterName("p0"));
            outputParam.SetupGet(o => o.ParameterName).Returns(outputParameterName);
            outputParam.Setup(o => o.GetDeclareLine())
                .Returns(declareP0);
            outputParam.Setup(o => o.PrepareOutputParameter(1))
                .Returns(prepared);
            outputParam.Setup(o => o.SetOutputValue(null)).Verifiable();


            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, wrapperFactoryMock.Object, metadataFactoryMock.Object);

            var result = service.ExecuteFunction<bool>(function, inputParam.Object, outputParam.Object);

            commandMock.Verify();
            outputParam.Verify();
            Assert.Equal(returnValue, result);
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(3, commandMock.Object.Parameters.Count);
        }

        #endregion BooleanParam

        #endregion ExecuteFunction
    }
}
