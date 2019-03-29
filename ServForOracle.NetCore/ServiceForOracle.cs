using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.Metadata;
using System.Threading.Tasks;
using ServForOracle.NetCore.Extensions;
using System.Data.Common;
using System.Diagnostics;
using ServForOracle.NetCore.OracleAbstracts;
using System.Runtime.CompilerServices;
using ServForOracle.NetCore.Cache;
using Microsoft.Extensions.Logging;
using ServForOracle.NetCore.Wrapper;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("ServForOracle.NetCore.UnitTests")]
namespace ServForOracle.NetCore
{
    public class ServiceForOracle : IServiceForOracle
    {
        private readonly IDbConnectionFactory _DbFactory;
        private readonly ILogger _Logger;
        private readonly MetadataOracleCommon _Common;
        private readonly IMetadataFactory _MetadataFactory;
        private readonly IMetadataBuilderFactory _BuilderFactory;
        private readonly IOracleRefCursorWrapperFactory _RefCursorWrapperFactory;

        public ServiceForOracle(ILogger<ServiceForOracle> logger, ServForOracleCache cache, string connectionString)
            : this(logger, cache, new OracleDbConnectionFactory(connectionString))
        {
        }
        public ServiceForOracle(ILogger<ServiceForOracle> logger, ServForOracleCache cache, IDbConnectionFactory factory)
           : this(logger, factory, new MetadataBuilderFactory(cache, logger), new OracleRefCursorWrapperFactory(), new MetadataFactory())
        {
        }

        internal ServiceForOracle(ILogger<ServiceForOracle> logger, IDbConnectionFactory factory, IMetadataBuilderFactory builderFactory, IMetadataFactory metadataFactory)
            : this(logger, factory, builderFactory, new OracleRefCursorWrapperFactory(), metadataFactory)
        {
        }

        internal ServiceForOracle(ILogger<ServiceForOracle> logger, IDbConnectionFactory factory, IMetadataBuilderFactory builderFactory, IOracleRefCursorWrapperFactory wrapperFactory, IMetadataFactory metadataFactory)
        {
            _Logger = logger;
            _DbFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            _MetadataFactory = metadataFactory ?? throw new ArgumentNullException(nameof(metadataFactory));
            _Common = _MetadataFactory.CreateCommon();
            _BuilderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
            _RefCursorWrapperFactory = wrapperFactory ?? throw new ArgumentNullException(nameof(wrapperFactory));
        }

        public async Task ExecuteProcedureAsync(string procedure, params IParam[] parameters)
        {
            try
            {
                using (var connection = _DbFactory.CreateConnection())
                {
                    var builder = _BuilderFactory.CreateBuilder(connection);
                    await ExecuteAsync(builder, connection, procedure, parameters).ConfigureAwait(false);
                }
            }
            finally
            {
                _DbFactory.Dispose();
            }
        }

        public void ExecuteProcedure(string procedure, params IParam[] parameters)
        {
            try
            {
                using (var connection = _DbFactory.CreateConnection())
                {
                    var builder = _BuilderFactory.CreateBuilder(connection);
                    Execute(builder, connection, procedure, parameters);
                }
            }
            finally
            {
                _DbFactory.Dispose();
            }
        }

        public async Task<T> ExecuteFunctionAsync<T>(string function, params IParam[] parameters)
        {
            return await ExecuteFunctionAsync<T>(function, null, parameters).ConfigureAwait(false);
        }

        public T ExecuteFunction<T>(string function, params IParam[] parameters)
        {
            return ExecuteFunction<T>(function, null, parameters);
        }

        public async Task<T> ExecuteFunctionAsync<T>(string function, OracleUdtInfo udtInfo, params IParam[] parameters)
        {
            MetadataBase returnMetadata = null;
            OracleParameter retOra = null;
            try
            {
                using (var connection = _DbFactory.CreateConnection())
                {
                    var builder = _BuilderFactory.CreateBuilder(connection);
                    await ExecuteAsync(builder, connection, function, parameters,
                        (info) => Task.FromResult(FunctionBeforeQuery<T>(builder, info, udtInfo, out returnMetadata,
                            out retOra)),
                        (info) =>
                        {
                            if (returnMetadata is MetadataOracleObject<T> metadata)
                            {
                                return Task.FromResult(
                                    ReturnValueAdditionalInformation(info, udtInfo, metadata, out retOra));
                            }
                            else if (returnMetadata is MetadataOracleBoolean metadataBoolean)
                            {
                                return Task.FromResult(
                                    ReturnValueAdditionalInformationBoolean<T>(info, metadataBoolean, out retOra));
                            }
                            else
                            {
                                return Task.FromResult<AdditionalInformation>(null);
                            }
                        }).ConfigureAwait(false);

                    return GetReturnParameterOtuputValue<T>(retOra, returnMetadata);
                }
            }
            finally { _DbFactory.Dispose(); }
        }

        public T ExecuteFunction<T>(string function, OracleUdtInfo udtInfo, params IParam[] parameters)
        {
            MetadataBase returnMetadata = null;
            OracleParameter retOra = null;
            try
            {
                using (var connection = _DbFactory.CreateConnection())
                {
                    var builder = _BuilderFactory.CreateBuilder(connection);

                    Execute(builder, connection, function, parameters,
                        (info) => FunctionBeforeQuery<T>(builder, info, udtInfo, out returnMetadata, out retOra),
                        (info) =>
                        {
                            if (returnMetadata is MetadataOracleObject<T> metadata)
                            {
                                return ReturnValueAdditionalInformation(info, udtInfo, metadata, out retOra);
                            }
                            else if (returnMetadata is MetadataOracleBoolean metadataBoolean)
                            {
                                return ReturnValueAdditionalInformationBoolean<T>(info, metadataBoolean, out retOra);
                            }
                            else
                            {
                                return null;
                            }
                        });

                    return GetReturnParameterOtuputValue<T>(retOra, returnMetadata);
                }
            }
            finally { _DbFactory.Dispose(); }
        }

        private AdditionalInformation ReturnValueAdditionalInformationBoolean<T>(ExecutionInformation info,
            MetadataOracleBoolean metadata, out OracleParameter parameter)
        {
            parameter = FunctionReturnOracleParameter<T>(info, metadata);
            var name = "ret";
            return new AdditionalInformation
            {
                Declare = metadata.GetDeclareLine(name),
                Output = metadata.OutputString(info.ParameterCounter, name)
            };
        }

        private AdditionalInformation ReturnValueAdditionalInformation<T>(ExecutionInformation info, OracleUdtInfo udt,
            MetadataOracleObject<T> metadata, out OracleParameter parameter)
        {
            var returnType = typeof(T);
            parameter = FunctionReturnOracleParameter<T>(info, metadata);
            var name = "ret";

            var returnInfo = new AdditionalInformation
            {
                Declare = metadata.GetDeclareLine(returnType, name, udt ?? metadata.OracleTypeNetMetadata.UDTInfo),
                Output = metadata.GetRefCursorQuery(info.ParameterCounter, name)
            };

            return returnInfo;
        }

        private string FunctionBeforeQuery<T>(MetadataBuilder builder, ExecutionInformation info, OracleUdtInfo udt, out MetadataBase metadata, out OracleParameter parameter)
        {
            if (typeof(T).IsBoolean())
            {
                metadata = _MetadataFactory.CreateBoolean();
                parameter = null;
                return "ret := ";
            }
            else if (typeof(T).IsClrType())
            {
                metadata = new MetadataBase();
                parameter = FunctionReturnOracleParameter<T>(info, metadata);
                return $"{parameter.ParameterName} := ";
            }
            else
            {
                metadata = builder.GetOrRegisterMetadataOracleObject<T>(udt);
                parameter = null;
                return "ret := ";
            }
        }

        private T GetReturnParameterOtuputValue<T>(OracleParameter retOra, MetadataBase returnMetadata = null)
        {
            var returnType = typeof(T);
            if (!returnType.IsClrType() && returnMetadata is MetadataOracleObject<T> metadata)
            {
                return (T)metadata.GetValueFromRefCursor(returnType, _RefCursorWrapperFactory.Create(retOra.Value as OracleRefCursor));

            }
            else if (returnMetadata is MetadataOracleBoolean metadataBoolean)
            {
                return (T)metadataBoolean.GetBooleanValue(retOra.Value);
            }
            else
            {

                return (T)_Common.ConvertOracleParameterToBaseType(returnType, retOra);
            }
        }

        private OracleParameter FunctionReturnOracleParameter<T>(ExecutionInformation info, MetadataBase metadata)
        {
            OracleParameter retOra;
            var type = typeof(T);
            if (type.IsBoolean())
            {
                retOra = new OracleParameter
                {
                    ParameterName = $":{info.ParameterCounter}",
                    OracleDbType = OracleDbType.Byte,
                    Direction = ParameterDirection.Output
                };
            }
            else if (type.IsClrType())
            {
                retOra = _Common.GetOracleParameter(
                    type: type, direction: ParameterDirection.Output, name: $":{info.ParameterCounter++}", value: DBNull.Value);
            }
            else
            {
                retOra = new OracleParameter($":{info.ParameterCounter}", DBNull.Value)
                {
                    OracleDbType = OracleDbType.RefCursor
                };
            }

            info.OracleParameterList.Add(retOra);
            return retOra;
        }

        private async Task ExecuteAsync(MetadataBuilder builder, DbConnection connection, string method, IParam[] parameters,
            Func<ExecutionInformation, Task<string>> beforeQuery = null,
            Func<ExecutionInformation, Task<AdditionalInformation>> beforeEnd = null)
        {
            _Logger?.LogInformation("Executing asynchroniously {method}", method);

            await LoadObjectParametersMetadataAsync(builder, parameters).ConfigureAwait(false);

            var info = new ExecutionInformation();
            var declare = ProcessDeclaration(parameters);
            var body = ProcessBody(parameters, info);

            //elvis operator is throwing null reference
            string beforeQ = string.Empty;
            if (beforeQuery != null)
            {
                beforeQ = await beforeQuery.Invoke(info).ConfigureAwait(false);
            }
            var query = new StringBuilder(beforeQ);
            query.AppendLine(ProcessQuery(method, parameters, info));

            var outparameters = ProcessOutputParameters(parameters, info);

            AdditionalInformation additionalInfo = null;

            if (beforeEnd != null)
            {
                additionalInfo = await beforeEnd.Invoke(info).ConfigureAwait(false);
            }

            if (additionalInfo != null)
            {
                declare.AppendLine(additionalInfo.Declare);
                outparameters.AppendLine(additionalInfo.Output);
            }

            var execute = PrepareStatement(declare.ToString(), body, query.ToString(), outparameters.ToString());

            await ExecuteNonQueryAsync(connection, info.OracleParameterList, execute).ConfigureAwait(false);

            await ProcessOutParametersAsync(info.Outputs).ConfigureAwait(false);

        }

        private void Execute(MetadataBuilder builder, DbConnection connection, string method, IParam[] parameters,
            Func<ExecutionInformation, string> beforeQuery = null,
            Func<ExecutionInformation, AdditionalInformation> beforeEnd = null)
        {
            _Logger?.LogInformation("Executing {method}", method);

            LoadObjectParametersMetadata(builder, parameters);

            var info = new ExecutionInformation();
            var declare = ProcessDeclaration(parameters);
            var body = ProcessBody(parameters, info);

            var query = new StringBuilder(beforeQuery?.Invoke(info));
            query.AppendLine(ProcessQuery(method, parameters, info));

            var outparameters = ProcessOutputParameters(parameters, info);

            var additionalInfo = beforeEnd?.Invoke(info);
            if (additionalInfo != null)
            {
                declare.AppendLine(additionalInfo.Declare);
                outparameters.AppendLine(additionalInfo.Output);
            }

            var execute = PrepareStatement(declare.ToString(), body, query.ToString(), outparameters.ToString());

            ExecuteNonQuery(connection, info.OracleParameterList, execute);

            ProcessOutParameters(info.Outputs);
        }

        private string PrepareStatement(string declare, string body, string query, string outparameters)
        {
            var execute = new StringBuilder();
            execute.AppendLine(declare);
            execute.AppendLine(body);
            execute.AppendLine(query);
            execute.AppendLine(outparameters);
            execute.Append("end;");
            return execute.ToString();
        }

        private async Task ProcessOutParametersAsync(List<PreparedOutputParameter> outputs)
        {
            var taskList = new List<Task>(outputs.Count);
            foreach (var param in outputs)
            {
                taskList.Add(param.Parameter.SetOutputValueAsync(param.OracleParameter.Value));
            }

            await Task.WhenAll(taskList.ToArray()).ConfigureAwait(false);
        }

        private void ProcessOutParameters(List<PreparedOutputParameter> outputs)
        {
            Parallel.ForEach(outputs, (param) =>
            {
                param.Parameter.SetOutputValue(param.OracleParameter.Value);
            });
        }

        private async Task ExecuteNonQueryAsync(DbConnection con, List<OracleParameter> oracleParameterList, string execute)
        {
            if (con.State != ConnectionState.Open)
            {
                await con.OpenAsync().ConfigureAwait(false);
            }

            var cmd = con.CreateCommand();
            cmd.Parameters.AddRange(oracleParameterList.ToArray());
            cmd.CommandText = execute;

            _Logger?.LogDebug("Executing asynchronously \r\n {execute}", execute);

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private void ExecuteNonQuery(DbConnection connection, List<OracleParameter> oracleParameterList, string execute)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var cmd = connection.CreateCommand();
            cmd.Parameters.AddRange(oracleParameterList.ToArray());
            cmd.CommandText = execute;

            _Logger?.LogDebug("Executing \r\n {execute}", execute);
            cmd.ExecuteNonQuery();
        }

        private async Task LoadObjectParametersMetadataAsync(MetadataBuilder builder, IParam[] parameters)
        {
            var tasksList = new List<Task>(parameters.Length);
            foreach (var param in parameters.Where(c => c is ParamObject).Cast<ParamObject>())
            {
                tasksList.Add(param.LoadObjectMetadataAsync(builder));
            }

            await Task.WhenAll(tasksList.ToArray()).ConfigureAwait(false);
        }

        private void LoadObjectParametersMetadata(MetadataBuilder builder, IParam[] parameters)
        {
            Parallel.ForEach(parameters.Where(c => c is ParamObject).Cast<ParamObject>(), param =>
            {
                param.LoadObjectMetadata(builder);
            });
        }

        private StringBuilder ProcessDeclaration(IParam[] parameters)
        {
            var declaration = new StringBuilder();
            var objCounter = 0;

            declaration.AppendLine("declare");

            foreach (var param in parameters.Where(c => c is ParamManaged).Cast<ParamManaged>())
            {
                if (param is ParamBoolean boolean && !(boolean.Direction == ParameterDirection.Output || boolean.Direction == ParameterDirection.InputOutput))
                {
                    continue;
                }

                var name = $"p{objCounter++}";
                param.SetParameterName(name);

                declaration.AppendLine(param.GetDeclareLine());
            }

            return declaration;
        }

        private string ProcessBody(IParam[] parameters, ExecutionInformation info)
        {
            var body = new StringBuilder();
            body.AppendLine("begin");

            foreach (var param in parameters.Where(c => c is ParamManaged))
            {
                if (param is ParamObject paramObj &&
                    (paramObj.Direction == ParameterDirection.Input || paramObj.Direction == ParameterDirection.InputOutput))
                {
                    var (constructor, lastNumber) = paramObj.BuildQueryConstructorString(info.ParameterCounter);
                    var oraParameters = paramObj.GetOracleParameters(info.ParameterCounter);

                    info.OracleParameterList.AddRange(oraParameters);

                    body.AppendLine(constructor);
                    info.ParameterCounter = lastNumber;
                }
                else if (param is ParamBoolean paramBool && param.Direction == ParameterDirection.InputOutput)
                {
                    body.AppendLine(paramBool.GetBodyVariableSetString());
                }
            }

            return body.ToString();
        }

        private StringBuilder ProcessOutputParameters(IParam[] parameters, ExecutionInformation info)
        {
            var outparameters = new StringBuilder();
            foreach (var param in parameters.Where(c => c is ParamManaged).Cast<ParamManaged>()
                .Where(c => c.Direction == ParameterDirection.Output || c.Direction == ParameterDirection.InputOutput))
            {
                var preparedOutput = param.PrepareOutputParameter(info.ParameterCounter++);
                outparameters.AppendLine(preparedOutput.OutputString);

                info.OracleParameterList.Add(preparedOutput.OracleParameter);
                info.Outputs.Add(preparedOutput);
            }

            return outparameters;
        }

        private string ProcessQuery(string method, IParam[] parameters, ExecutionInformation info)
        {
            var query = new StringBuilder(method + "(");
            bool first = true;
            foreach (var param in parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(",");
                }
                if (param is ParamObject paramObject)
                {
                    query.Append(paramObject.ParameterName);
                }
                else if (param is ParamClrType clrType)
                {
                    var name = $":{info.ParameterCounter++}";
                    query.Append(name);
                    var oracleParameter = clrType.GetOracleParameter(name);
                    info.OracleParameterList.Add(oracleParameter);
                    if (clrType.Direction == ParameterDirection.Output || clrType.Direction == ParameterDirection.InputOutput)
                    {
                        info.Outputs.Add(new PreparedOutputParameter(clrType, oracleParameter, null));
                    }
                }
                else if (param is ParamBoolean boolean)
                {
                    if (boolean.Direction == ParameterDirection.Output || boolean.Direction == ParameterDirection.InputOutput)
                    {
                        query.Append(boolean.ParameterName);
                    }
                    else
                    {
                        var name = $":{info.ParameterCounter}";
                        boolean.SetParameterName(name);
                        info.OracleParameterList.Add(boolean.GetOracleParameter(info.ParameterCounter++));
                        query.Append(name);
                    }
                }
            }

            query.Append(");");

            return query.ToString();
        }
    }

    internal class ExecutionInformation
    {
        public ExecutionInformation()
        {
            OracleParameterList = new List<OracleParameter>();
            Outputs = new List<PreparedOutputParameter>();
            ParameterCounter = 0;
        }

        public List<PreparedOutputParameter> Outputs { get; set; }
        public int ParameterCounter { get; set; }
        public List<OracleParameter> OracleParameterList { get; set; }
    }

    internal class AdditionalInformation
    {
        public string Declare { get; set; }
        public string Output { get; set; }
    }
}
