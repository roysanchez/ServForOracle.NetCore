using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using ServForOracle.NetCore.UnitTests.TestTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ServiceForOracleTests
    {
        public class TestClass
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
        }
        #region Constructor

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameters_ConnectionString(ILogger<ServiceForOracle> logger, ServForOracleCache cache, string connectionString)
        {
            var service = new ServiceForOracle(logger, cache, connectionString);
        }

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameters_DbFactory(ILogger<ServiceForOracle> logger, ServForOracleCache cache, IDbConnectionFactory factory)
        {
            var service = new ServiceForOracle(logger, cache, factory);
        }

        [Theory, CustomAutoData]
        internal void Constructor_FourParameters(ILogger<ServiceForOracle> logger, IDbConnectionFactory factory, MetadataOracleCommon common, IMetadataBuilderFactory builderFactory)
        {
            var service = new ServiceForOracle(logger, factory, builderFactory, common);
        }

        [Theory, CustomAutoData]
        internal void Constructor_FourParameters_NullParameter_ThrowsArgumentNull(ILogger<ServiceForOracle> logger, IDbConnectionFactory factory, MetadataOracleCommon common, IMetadataBuilderFactory builderFactory)
        {
            Assert.Throws<ArgumentNullException>("factory", () => new ServiceForOracle(logger, null, builderFactory, common));
            Assert.Throws<ArgumentNullException>("builderFactory", () => new ServiceForOracle(logger, factory, null, common));
            Assert.Throws<ArgumentNullException>("common", () => new ServiceForOracle(logger, factory, builderFactory, null));
        }

        #endregion Constructor

        #region ExecuteProcedureAsync

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_NoParams_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock)
        {
            var message = $"declare{Environment.NewLine}{Environment.NewLine}begin"
                + $"{Environment.NewLine}{Environment.NewLine}{procedure}();"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure);

            commandMock.Verify();
            Assert.Equal(message, commandMock.Object.CommandText);
            Assert.Empty(commandMock.Object.Parameters);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> inputMock)
        {
            var message = $"declare{Environment.NewLine}{Environment.NewLine}begin"
                + $"{Environment.NewLine}{Environment.NewLine}{procedure}(:0);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);


            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, inputMock.Object);

            commandMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            var parameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.NotNull(parameter);
            Assert.Equal(":0", parameter.ParameterName);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(inputMock.Object.Value, parameter.Value);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneOutputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> outputMock)
        {
            var expectedValue = outputMock.Object.Value;
            var message = $"declare{Environment.NewLine}{Environment.NewLine}begin"
                + $"{Environment.NewLine}{Environment.NewLine}{procedure}(:0);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";
            
            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            outputMock.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            
            outputMock.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
                .Verifiable();
            
            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, outputMock.Object);

            commandMock.Verify();
            outputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            var parameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.NotNull(parameter);
            Assert.Equal(":0", parameter.ParameterName);
            Assert.Equal(ParameterDirection.Output, parameter.Direction);
            Assert.Equal(expectedValue, parameter.Value);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputOutputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> inputOutputMock, string output)
        {
            var expectedValue = new OracleParameter { Value = output };
            var message = $"declare{Environment.NewLine}{Environment.NewLine}begin"
                + $"{Environment.NewLine}{Environment.NewLine}{procedure}(:0);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            inputOutputMock.SetupGet(io => io.Direction).Returns(ParameterDirection.InputOutput);
            inputOutputMock.Setup(io => io.GetOracleParameter(":0"))
                .Returns(expectedValue);

            inputOutputMock.Setup(io => io.SetOutputValueAsync(output))
                .Returns(Task.CompletedTask)
                .Verifiable();
           
            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, inputOutputMock.Object);

            commandMock.Verify();
            inputOutputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            var parameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.NotNull(parameter);
            Assert.Equal(expectedValue, parameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputOneOutputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> inputMock, Mock<ParamClrType<string>> outputMock, string input, string output)
        {
            var inputParameter = new OracleParameter { Value = input };
            var outputParameter = new OracleParameter { Value = output };
            var message = $"declare{Environment.NewLine}{Environment.NewLine}begin"
                + $"{Environment.NewLine}{Environment.NewLine}{procedure}(:0,:1);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            inputMock.Setup(io => io.GetOracleParameter(":0"))
                .Returns(inputParameter);
            outputMock.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            outputMock.Setup(io => io.GetOracleParameter(":1"))
                .Returns(outputParameter);

            outputMock.Setup(io => io.SetOutputValueAsync(output))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, inputMock.Object, outputMock.Object);

            commandMock.Verify();
            outputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);


            Assert.NotEmpty(commandMock.Object.Parameters);

            AssertExtensions.Collection(commandMock.Object.Parameters,
                c =>
                {
                    Assert.NotNull(c);
                    var parameter = Assert.IsType<OracleParameter>(c);
                    Assert.Equal(inputParameter, parameter);
                },
                c =>
                {
                    Assert.NotNull(c);
                    var parameter = Assert.IsType<OracleParameter>(c);
                    Assert.Equal(outputParameter, parameter);
                }
            );
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputObjectParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamObject<TestClass>> inputMock, string declareLine, string constructor, int lastNumber)
        {
            var inputParameters = new OracleParameter[2];

            var message = $"declare{Environment.NewLine}"
                + $"{declareLine}{Environment.NewLine}"
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + $"{constructor}{Environment.NewLine}{Environment.NewLine}"
                + $"{procedure}(p0);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputMock.Setup(i => i.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            inputMock.Setup(i => i.GetDeclareLine())
                .Returns(declareLine);
            inputMock.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputMock.Setup(i => i.GetOracleParameters(0))
                .Returns(inputParameters);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, inputMock.Object);

            commandMock.Verify();
            
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.Length(commandMock.Object.Parameters, inputParameters.Length);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneOutputObjectParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamObject<TestClass>> outputMock, string declareLine, string outputString)
        {
            var prepared = new PreparedOutputParameter(outputMock.Object, new OracleParameter(), outputString);

            var message = $"declare{Environment.NewLine}"
                + $"{declareLine}{Environment.NewLine}"
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + Environment.NewLine
                + $"{procedure}(p0);{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            outputMock.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputMock.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            outputMock.Setup(o => o.GetDeclareLine())
                .Returns(declareLine);
            outputMock.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputMock.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, outputMock.Object);

            commandMock.Verify();
            outputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(prepared.OracleParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputOneOutputObjectParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock,
            Mock<ParamObject<TestClass>> inputMock, Mock<ParamObject<TestClass>> outputMock, string declareP0, string declareP1, string outputString, string constructor, int lastNumber)
        {
            var prepared = new PreparedOutputParameter(outputMock.Object, new OracleParameter(), outputString);
            var inputParameters = new OracleParameter[2];

            var message = $"declare{Environment.NewLine}"
                + $"{declareP0}{Environment.NewLine}" //p0
                + $"{declareP1}{Environment.NewLine}" //p1
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + $"{constructor}{Environment.NewLine}{Environment.NewLine}"
                + $"{procedure}(p0,p1);{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            outputMock.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputMock.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            outputMock.Setup(o => o.GetDeclareLine())
                .Returns(declareP1);
            outputMock.Setup(o => o.PrepareOutputParameter(lastNumber))
                .Returns(prepared);
            outputMock.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            inputMock.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputMock.Setup(i => i.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            inputMock.Setup(i => i.GetDeclareLine())
                .Returns(declareP0);
            inputMock.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputMock.Setup(i => i.GetOracleParameters(0))
                .Returns(inputParameters);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, inputMock.Object, outputMock.Object);

            commandMock.Verify();
            outputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.Length(commandMock.Object.Parameters, 3);
            var oracleParameter = Assert.IsType<OracleParameter>(commandMock.Object.Parameters[2]);
            Assert.Equal(prepared.OracleParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputBooleanParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamBoolean> inputMock)
        {
            var inputParameter = new OracleParameter();

            var message = $"declare{Environment.NewLine}"
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + Environment.NewLine
                + $"{procedure}(:0);"
                + $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);

            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputMock.Setup(i => i.GetOracleParameter(0))
                .Returns(inputParameter);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, inputMock.Object);

            commandMock.Verify();

            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(inputParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneOutputBooleanParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamBoolean> outputMock, string declareLine, string outputString)
        {
            var prepared = new PreparedOutputParameter(outputMock.Object, new OracleParameter(), outputString);

            var message = $"declare{Environment.NewLine}"
                + $"{declareLine}{Environment.NewLine}"
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                //+ $"{bodyString}{Environment.NewLine}"
                + Environment.NewLine
                + $"{procedure}(p0);{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            outputMock.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            
            outputMock.Setup(o => o.GetDeclareLine())
                .Returns(declareLine);
            outputMock.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputMock.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, outputMock.Object);

            commandMock.Verify();
            outputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(prepared.OracleParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputOutputBooleanParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, MetadataOracleCommon common, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamBoolean> inputOutputMock, string declareLine, string outputString, string bodyString)
        {
            var prepared = new PreparedOutputParameter(inputOutputMock.Object, new OracleParameter(), outputString);

            var message = $"declare{Environment.NewLine}"
                + $"{declareLine}{Environment.NewLine}"
                + $"{Environment.NewLine}begin{Environment.NewLine}"
                + $"{bodyString}{Environment.NewLine}"
                + Environment.NewLine
                + $"{procedure}(p0);{Environment.NewLine}{Environment.NewLine}"
                + $"{outputString}{Environment.NewLine}{Environment.NewLine}end;";

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.Create(connectionMock.Object)).Returns(builderMock.Object);

            inputOutputMock.Setup(o => o.Direction).Returns(ParameterDirection.InputOutput);

            inputOutputMock.Setup(o => o.GetDeclareLine())
                .Returns(declareLine);
            inputOutputMock.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            inputOutputMock.Setup(o => o.GetBodyVariableSetString())
                .Returns(bodyString);
            inputOutputMock.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, common);

            await service.ExecuteProcedureAsync(procedure, inputOutputMock.Object);

            commandMock.Verify();
            inputOutputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(prepared.OracleParameter, oracleParameter);
        }

        #endregion ExecuteProcedureAsync

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

        [Theory, CustomAutoData]
        internal async Task ExecuteFunctionAsync_StringReturn_OneParamInput(string function, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<TestDbConnection> connectionMock, Mock<MetadataOracleCommon> commonMock, OracleParameter oracleParameter, Mock<ParamClrType<string>> inputParam, string expectedValue)
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

        #endregion ExecuteFunctionAsync
    }
}
