using AutoFixture;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
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
    public class ServiceForOracle_ProcedureTests
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


        #region ExecuteProcedureAsync

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_NoParams_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure);

            commandMock.Verify();
            Assert.Equal(message, commandMock.Object.CommandText);
            Assert.Empty(commandMock.Object.Parameters);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> inputMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

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
        internal async Task ExecuteProcedureAsync_OneOutputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> outputMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            outputMock.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);

            outputMock.Setup(o => o.SetOutputValueAsync(expectedValue)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

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
        internal async Task ExecuteProcedureAsync_OneInputOutputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> inputOutputMock, string output)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            inputOutputMock.SetupGet(io => io.Direction).Returns(ParameterDirection.InputOutput);
            inputOutputMock.Setup(io => io.GetOracleParameter(":0"))
                .Returns(expectedValue);

            inputOutputMock.Setup(io => io.SetOutputValueAsync(output))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure, inputOutputMock.Object);

            commandMock.Verify();
            inputOutputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            var parameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.NotNull(parameter);
            Assert.Equal(expectedValue, parameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputOneOutputClrParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamClrType<string>> inputMock, Mock<ParamClrType<string>> outputMock, string input, string output)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.SetupGet(i => i.Direction).Returns(ParameterDirection.Input);
            inputMock.Setup(io => io.GetOracleParameter(":0"))
                .Returns(inputParameter);
            outputMock.SetupGet(o => o.Direction).Returns(ParameterDirection.Output);
            outputMock.Setup(io => io.GetOracleParameter(":1"))
                .Returns(outputParameter);

            outputMock.Setup(io => io.SetOutputValueAsync(output))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

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
        internal async Task ExecuteProcedureAsync_OneInputObjectParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamObject<TestClass>> inputMock, string declareLine, string constructor, int lastNumber)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputMock.Setup(i => i.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            inputMock.Setup(i => i.GetDeclareLine())
                .Returns(declareLine);
            inputMock.Setup(i => i.BuildQueryConstructorString(0))
                .Returns((constructor, lastNumber));
            inputMock.Setup(i => i.GetOracleParameters(0))
                .Returns(inputParameters);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure, inputMock.Object);

            commandMock.Verify();

            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            AssertExtensions.Length(commandMock.Object.Parameters, inputParameters.Length);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneOutputObjectParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamObject<TestClass>> outputMock, string declareLine, string outputString)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            outputMock.Setup(o => o.Direction).Returns(ParameterDirection.Output);
            outputMock.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object))
                .Returns(Task.CompletedTask);
            outputMock.Setup(o => o.GetDeclareLine())
                .Returns(declareLine);
            outputMock.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputMock.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure, outputMock.Object);

            commandMock.Verify();
            outputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(prepared.OracleParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputOneOutputObjectParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock,
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

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

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

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
        internal async Task ExecuteProcedureAsync_OneInputBooleanParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamBoolean> inputMock)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            inputMock.Setup(i => i.Direction).Returns(ParameterDirection.Input);
            inputMock.Setup(i => i.GetOracleParameter(0))
                .Returns(inputParameter);

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure, inputMock.Object);

            commandMock.Verify();

            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(inputParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneOutputBooleanParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamBoolean> outputMock, string declareLine, string outputString)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            outputMock.Setup(o => o.Direction).Returns(ParameterDirection.Output);

            outputMock.Setup(o => o.GetDeclareLine())
                .Returns(declareLine);
            outputMock.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            outputMock.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure, outputMock.Object);

            commandMock.Verify();
            outputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(prepared.OracleParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneInputOutputBooleanParam_Works(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Mock<ParamBoolean> inputOutputMock, string declareLine, string outputString, string bodyString)
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
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            inputOutputMock.Setup(o => o.Direction).Returns(ParameterDirection.InputOutput);

            inputOutputMock.Setup(o => o.GetDeclareLine())
                .Returns(declareLine);
            inputOutputMock.Setup(o => o.PrepareOutputParameter(0))
                .Returns(prepared);
            inputOutputMock.Setup(o => o.GetBodyVariableSetString())
                .Returns(bodyString);
            inputOutputMock.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask)
                .Verifiable();

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure, inputOutputMock.Object);

            commandMock.Verify();
            inputOutputMock.Verify();
            Assert.Equal(commandMock.Object.CommandText, message);
            Assert.NotEmpty(commandMock.Object.Parameters);
            var oracleParameter = Assert.IsType<OracleParameter>(Assert.Single(commandMock.Object.Parameters));
            Assert.Equal(prepared.OracleParameter, oracleParameter);
        }

        [Theory, CustomAutoData]
        internal async Task ExecuteProcedureAsync_OneOfEveryKindParam(string procedure, Mock<IDbConnectionFactory> dbConnectionFactoryMock, ILogger<ServiceForOracle> logger, Mock<IMetadataBuilderFactory> builderFactoryMock, IMetadataFactory metadataFactory, Mock<TestDbConnection> connectionMock, Mock<MetadataBuilder> builderMock, Mock<TestDbCommand> commandMock, Fixture fixture)
        {
            const string voidMethodName = "set_Value";
            const string parametersNameRegex = @"^p\d*$";
            const string oracleNameRegex = @"^:\d*$";
            var objectInputOutputLastNumber = fixture.Create<int>();

            commandMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();
            connectionMock.Setup(c => c._CreateDbCommand()).Returns(commandMock.Object);
            connectionMock.SetupGet(c => c._State).Returns(ConnectionState.Open);
            dbConnectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);
            builderFactoryMock.Setup(b => b.CreateBuilder(connectionMock.Object)).Returns(builderMock.Object);

            #region ParamClrtType
            var stringInput = new Mock<ParamClrType<string>>(MockBehavior.Strict, null, ParameterDirection.Input);

            stringInput
                .Protected()
                .SetupSequence(voidMethodName, ItExpr.IsNull<object>())
                .Pass();
            stringInput.SetupGet(s => s.Direction).Returns(ParameterDirection.Input);
            stringInput.SetupGet(s => s.Value).Returns(fixture.Create<string>());
            var declareStringInput = "declareStringInput" + fixture.Create<string>();
            var stringInputParameter = fixture.Create<OracleParameter>();
            stringInput.Setup(i => i.GetOracleParameter(It.IsRegex(oracleNameRegex))).Returns(stringInputParameter);


            var stringOutput = new Mock<ParamClrType<string>>(MockBehavior.Strict, null, ParameterDirection.Output);
            stringOutput
                .Protected()
                .SetupSequence(voidMethodName, ItExpr.IsNull<object>())
                .Pass();

            stringOutput.SetupGet(s => s.Direction).Returns(ParameterDirection.Output);
            var declareStringOutput = "declareStringOutput" + fixture.Create<string>();
            stringOutput.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask);
            var stringOutputParameter = fixture.Create<OracleParameter>();
            stringOutput.Setup(i => i.GetOracleParameter(It.IsRegex(oracleNameRegex))).Returns(stringOutputParameter);


            var stringInputOutput = new Mock<ParamClrType<string>>(MockBehavior.Strict, fixture.Create<string>(), ParameterDirection.InputOutput);
            stringInputOutput
                .Protected()
                .SetupSequence(voidMethodName, ItExpr.IsAny<object>())
                .Pass();

            stringInputOutput.SetupGet(s => s.Direction).Returns(ParameterDirection.InputOutput);
            var declareStringInputOutput = "declareStringInputOutput" + fixture.Create<string>();
            stringInputOutput.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask);
            var stringInputOutputParameter = fixture.Create<OracleParameter>();
            stringInputOutput.Setup(i => i.GetOracleParameter(It.IsRegex(oracleNameRegex))).Returns(stringInputOutputParameter);

            #endregion ParamClrtType

            #region ParamObject

            var objectInputValue = fixture.Create<TestClass>();
            var objectInput = new Mock<ParamObject<TestClass>>(MockBehavior.Strict, objectInputValue, ParameterDirection.Input);
            objectInput
                .Protected()
                .Setup(voidMethodName, ItExpr.Is<object>(v => v.Equals(objectInputValue)));
            objectInput
                .Setup(m => m.SetParameterName(It.IsRegex(parametersNameRegex)));

            objectInput.SetupGet(s => s.Direction).Returns(ParameterDirection.Input);
            var declareObjectInput = "declareObjectInput" + fixture.Create<string>();
            objectInput.Setup(x => x.GetDeclareLine()).Returns(declareObjectInput);
            var objectInputParameters = fixture.Create<OracleParameter[]>();
            objectInput.Setup(i => i.GetOracleParameters(It.IsAny<int>())).Returns(objectInputParameters);
            var objectInputLastNumber = fixture.Create<int>();
            var objectInputConstructor = "objectInputConstructor" + fixture.Create<string>();
            objectInput.Setup(i => i.BuildQueryConstructorString(It.IsAny<int>())).Returns((objectInputConstructor, objectInputLastNumber));
            objectInput.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object)).Returns(Task.CompletedTask);

            var objectInputParameterName = "objectInputParameterName" + fixture.Create<string>();
            objectInput.SetupGet(o => o.ParameterName)
                .Returns(objectInputParameterName);


            var objectOutput = new Mock<ParamObject<TestClass>>(MockBehavior.Strict, null, ParameterDirection.Output);
            objectOutput
                .Protected()
                .Setup(voidMethodName, ItExpr.IsNull<object>());
            objectOutput
                .Setup(m => m.SetParameterName(It.IsRegex(parametersNameRegex)));

            objectOutput.SetupGet(s => s.Direction).Returns(ParameterDirection.Output);
            var declareObjectOutput = "declareObjectOutput" + fixture.Create<string>();
            objectOutput.Setup(x => x.GetDeclareLine()).Returns(declareObjectOutput);

            var preparedObjectOutput = new PreparedOutputParameter(objectOutput.Object, fixture.Create<OracleParameter>(), "preparedObjectOutput" + fixture.Create<string>());
            objectOutput.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask);
            objectOutput.Setup(o => o.PrepareOutputParameter(It.IsAny<int>())).Returns(preparedObjectOutput);
            objectOutput.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object)).Returns(Task.CompletedTask);

            var objectOutputParameterName = "objectOutputParameterName" + fixture.Create<string>();
            objectOutput.SetupGet(o => o.ParameterName)
                .Returns(objectOutputParameterName);


            var objectInputOutputValue = fixture.Create<TestClass>();
            var objectInputOutput = new Mock<ParamObject<TestClass>>(MockBehavior.Strict, objectInputOutputValue, ParameterDirection.InputOutput);
            objectInputOutput
                .Protected()
                .Setup(voidMethodName, ItExpr.Is<object>(v => v.Equals(objectInputOutputValue)));
            objectInputOutput
                .Setup(m => m.SetParameterName(It.IsRegex(parametersNameRegex)));

            var objectInputOutputParameters = fixture.Create<OracleParameter[]>();
            objectInputOutput.Setup(i => i.GetOracleParameters(It.IsAny<int>())).Returns(objectInputOutputParameters);
            objectInputOutput.SetupGet(s => s.Direction).Returns(ParameterDirection.InputOutput);
            var declareObjectInputOutput = "declareObjectInputOutput" + fixture.Create<string>();
            objectInputOutput.Setup(x => x.GetDeclareLine()).Returns(declareObjectInputOutput);
            var preparedObjectInputOutput = new PreparedOutputParameter(objectInputOutput.Object, fixture.Create<OracleParameter>(), "preparedObjectInputOutput" + fixture.Create<string>());
            objectInputOutput.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask);
            objectInputOutput.Setup(o => o.PrepareOutputParameter(It.IsAny<int>())).Returns(preparedObjectInputOutput);


            var objectInputOutputConstructor = "objectInputOutputConstructor" + fixture.Create<string>();

            objectInputOutput.Setup(i => i.BuildQueryConstructorString(It.IsAny<int>())).Returns((objectInputOutputConstructor, objectInputOutputLastNumber));
            objectInputOutput.Setup(o => o.LoadObjectMetadataAsync(builderMock.Object)).Returns(Task.CompletedTask);

            var objectInputOutputParameterName = "objectInputOutputParameterName" + fixture.Create<string>();
            objectInputOutput.SetupGet(o => o.ParameterName)
                .Returns(objectInputOutputParameterName);

            #endregion ParamObject

            #region ParamBoolean

            var booleanInput = new Mock<ParamBoolean>(MockBehavior.Strict, false, ParameterDirection.Input);
            booleanInput
                .Protected()
                .Setup(voidMethodName, ItExpr.Is<object>(v => v.Equals(false)));
            booleanInput.SetupGet(s => s.Direction).Returns(ParameterDirection.Input);
            booleanInput
                .Setup(m => m.SetParameterName(It.IsRegex(oracleNameRegex)));

            var booleanInputParameter = fixture.Create<OracleParameter>();
            booleanInput.Setup(i => i.GetOracleParameter(It.IsAny<int>())).Returns(booleanInputParameter);

            var booleanOutput = new Mock<ParamBoolean>(MockBehavior.Strict, null, ParameterDirection.Output);
            booleanOutput
                .Protected()
                .Setup(voidMethodName, ItExpr.IsNull<object>());
            booleanOutput
                .Setup(m => m.SetParameterName(It.IsRegex(parametersNameRegex)));
            booleanOutput.SetupGet(s => s.Direction).Returns(ParameterDirection.Output);
            var booleanOutputParameterName = "booleanOutputParameterName" + fixture.Create<string>();
            booleanOutput
                .SetupGet(b => b.ParameterName)
                .Returns(booleanOutputParameterName);
            var declareBooleanOutput = "declareBooleanOutput" + fixture.Create<string>();
            booleanOutput.Setup(x => x.GetDeclareLine()).Returns(declareBooleanOutput);
            var preparedBooleanOutput = new PreparedOutputParameter(booleanOutput.Object, fixture.Create<OracleParameter>(), "preparedBooleanOutput" + fixture.Create<string>());
            booleanOutput.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask);
            booleanOutput.Setup(o => o.PrepareOutputParameter(It.IsAny<int>())).Returns(preparedBooleanOutput);

            var booleanInputOutput = new Mock<ParamBoolean>(MockBehavior.Strict, true, ParameterDirection.InputOutput);
            booleanInputOutput
                .Setup(m => m.SetParameterName(It.IsRegex(parametersNameRegex)));
            booleanInputOutput
                .Protected()
                .Setup(voidMethodName, ItExpr.Is<object>(v => v.Equals(true)));

            var booleanInputOutputParameterName = "booleanInputOutputParameterName" + fixture.Create<string>();
            booleanInputOutput
                .SetupGet(b => b.ParameterName)
                .Returns(booleanInputOutputParameterName);

            booleanInputOutput.SetupGet(s => s.Direction).Returns(ParameterDirection.InputOutput);
            var declareBooleanInputOutput = "declareBooleanInputOutput" + fixture.Create<string>();
            booleanInputOutput.Setup(x => x.GetDeclareLine()).Returns(declareBooleanInputOutput);
            var preparedInputOuputBoolean = new PreparedOutputParameter(booleanInputOutput.Object, fixture.Create<OracleParameter>(), "preparedInputOuputBoolean" + fixture.Create<string>());
            var booleanInputOutputBodyString = "booleanInputOutputBodyString" + fixture.Create<string>();
            booleanInputOutput.Setup(o => o.GetBodyVariableSetString()).Returns(booleanInputOutputBodyString);
            booleanInputOutput.Setup(o => o.SetOutputValueAsync(null)).Returns(Task.CompletedTask);
            booleanInputOutput.Setup(o => o.PrepareOutputParameter(It.IsAny<int>())).Returns(preparedInputOuputBoolean);

            #endregion ParamBoolean

            #region Message

            var message = new StringBuilder();
            message.AppendLine("declare");
            message.AppendLine(declareObjectInput);
            message.AppendLine(declareObjectOutput);
            message.AppendLine(declareBooleanOutput);
            message.AppendLine(declareObjectInputOutput);
            message.AppendLine(declareBooleanInputOutput);
            message.AppendLine();

            message.AppendLine("begin");
            message.AppendLine(objectInputConstructor);
            message.AppendLine(objectInputOutputConstructor);
            message.AppendLine(booleanInputOutputBodyString);

            message.AppendLine();
            message.Append(procedure + "(");
            message.Append(objectInputParameterName + ",");
            message.Append(":" + objectInputOutputLastNumber + ",");
            message.Append(":" + (objectInputOutputLastNumber + 1) + ",");
            message.Append(objectOutputParameterName + ",");
            message.Append(":" + (objectInputOutputLastNumber + 2) + ",");
            message.Append(booleanOutputParameterName + ",");
            message.Append(objectInputOutputParameterName + ",");
            message.Append(":" + (objectInputOutputLastNumber + 3) + ",");
            message.AppendLine(booleanInputOutputParameterName + ");");

            message.AppendLine();
            message.AppendLine(preparedObjectOutput.OutputString);
            message.AppendLine(preparedBooleanOutput.OutputString);
            message.AppendLine(preparedObjectInputOutput.OutputString);
            message.AppendLine(preparedInputOuputBoolean.OutputString);

            message.AppendLine();
            message.Append("end;");

            #endregion Message

            var service = new ServiceForOracle(logger, dbConnectionFactoryMock.Object, builderFactoryMock.Object, metadataFactory);

            await service.ExecuteProcedureAsync(procedure, objectInput.Object, stringInput.Object, booleanInput.Object, objectOutput.Object, stringOutput.Object, booleanOutput.Object, objectInputOutput.Object, stringInputOutput.Object, booleanInputOutput.Object);

            commandMock.Verify();
            booleanInputOutput.Verify();
            Assert.Equal(message.ToString(), commandMock.Object.CommandText);
            Assert.NotEmpty(commandMock.Object.Parameters);
            Assert.Equal(14, commandMock.Object.Parameters.Count);
        }

        #endregion ExecuteProcedureAsync
    }
}
