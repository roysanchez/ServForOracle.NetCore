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

namespace ServForOracle.NetCore
{
    public class ServiceForOracle : IServiceForOracle
    {
        private readonly OracleConnection _Connection;
        private readonly MetadataBuilder _Builder;

        public ServiceForOracle(OracleConnection connection)
        {
            _Connection = connection;
            _Builder = new MetadataBuilder(connection);
        }

        public async Task ExecuteProcedureAsync(string procedure, params IParam[] parameters)
        {
            await ExecuteAsync(procedure, parameters);
        }

        public void ExecuteProcedure(string procedure, params IParam[] parameters)
        {
            Execute(procedure, parameters);
        }

        public async Task<T> ExecuteFunctionAsync<T>(string function, params IParam[] parameters)
        {
            return await ExecuteFunctionAsync<T>(function, null, parameters);
        }

        public T ExecuteFunction<T>(string function, params IParam[] parameters)
        {
            return ExecuteFunction<T>(function, null, parameters);
        }

        public async Task<T> ExecuteFunctionAsync<T>(string function, OracleUdtInfo udtInfo, params IParam[] parameters)
        {
            var returnType = typeof(T);
            MetadataOracle returnMetadata = null;
            OracleParameter retOra = null;

            await ExecuteAsync($"ret := {function}", parameters, (info) =>
            {
                return Task.FromResult(FunctionBeforeQuery<T>(info, udtInfo, out returnMetadata, out retOra));
            },
            (info) =>
            {
                if (returnMetadata is MetadataOracleObject<T> metadata)
                {
                    return Task.FromResult(ReturnValueAdditionalInformation(info, udtInfo, metadata, out retOra));
                }
                else
                {
                    return Task.FromResult(null as AdditionalInformation);
                }
            });

            return GetReturnParameterOtuputValue<T>(retOra, returnMetadata);
        }

        public T ExecuteFunction<T>(string function, OracleUdtInfo udtInfo, params IParam[] parameters)
        {
            var returnType = typeof(T);
            MetadataOracle returnMetadata = null;
            OracleParameter retOra = null;

            Execute(function, parameters, (info) => FunctionBeforeQuery<T>(info, udtInfo, out returnMetadata, out retOra),
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

        private AdditionalInformation ReturnValueAdditionalInformationBoolean<T>(ExecutionInformation info,
            MetadataOracleBoolean metadata, out OracleParameter parameter)
        {
            parameter = FunctionReturnOracleParameter<T>(info);
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
            parameter = FunctionReturnOracleParameter<T>(info);
            var name = "ret";

            var returnInfo = new AdditionalInformation
            {
                Declare = metadata.GetDeclareLine(returnType, name, udt ?? metadata.OracleTypeNetMetadata.UDTInfo),
                Output = metadata.GetRefCursorQuery(info.ParameterCounter, name)
            };

            return returnInfo;
        }

        private string FunctionBeforeQuery<T>(ExecutionInformation info, OracleUdtInfo udt, out MetadataOracle metadata, out OracleParameter parameter)
        {
            if (typeof(T).IsBoolean())
            {
                metadata = new MetadataOracleBoolean();
                parameter = null;
                return "ret := ";
            }
            else if (typeof(T).IsClrType())
            {
                parameter = FunctionReturnOracleParameter<T>(info);
                metadata = new MetadataOracle();
                return $"{parameter.ParameterName} := ";
            }
            else
            {
                metadata = _Builder.GetOrRegisterMetadataOracleObject<T>(udt);
                parameter = null;
                return "ret := ";
            }
        }

        private T GetReturnParameterOtuputValue<T>(OracleParameter retOra, MetadataOracle returnMetadata = null)
        {
            var returnType = typeof(T);
            if (!returnType.IsClrType() && returnMetadata is MetadataOracleObject<T> metadata)
            {
                return (T)metadata.GetValueFromRefCursor(returnType, retOra.Value as OracleRefCursor);

            }
            else if (returnMetadata is MetadataOracleBoolean metadataBoolean)
            {
                return (T)metadataBoolean.GetBooleanValue(retOra.Value);
            }
            else
            {
                return (T)returnMetadata.ConvertOracleParameterToBaseType(returnType, retOra);
            }
        }

        private OracleParameter FunctionReturnOracleParameter<T>(ExecutionInformation info)
        {
            OracleParameter retOra;

            if (typeof(T).IsClrType())
            {
                retOra = new OracleParameter
                {
                    ParameterName = $":{info.ParameterCounter++}",
                    Direction = ParameterDirection.Output
                };
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

        private async Task ExecuteAsync(string method, IParam[] parameters,
            Func<ExecutionInformation, Task<string>> beforeQuery = null,
            Func<ExecutionInformation, Task<AdditionalInformation>> beforeEnd = null)
        {
            await LoadObjectParametersMetadataAsync(parameters);

            var info = new ExecutionInformation();
            var declare = ProcessDeclaration(parameters);
            var body = ProcessBody(parameters, info);

            var query = new StringBuilder(await beforeQuery?.Invoke(info));
            query.AppendLine(ProcessQuery(method, parameters, info));

            var outparameters = ProcessOutputParameters(parameters, info);

            var additionalInfo = await beforeEnd?.Invoke(info);
            if (additionalInfo != null)
            {
                declare.AppendLine(additionalInfo.Declare);
                outparameters.AppendLine(additionalInfo.Output);
            }

            var execute = PrepareStatement(declare.ToString(), body, query.ToString(), outparameters.ToString());

            await ExecuteNonQueryAsync(info.OracleParameterList, execute);

            await ProcessOutParametersAsync(info.Outputs);

        }

        private void Execute(string method, IParam[] parameters,
            Func<ExecutionInformation, string> beforeQuery = null,
            Func<ExecutionInformation, AdditionalInformation> beforeEnd = null)
        {
            LoadObjectParametersMetadata(parameters);

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

            ExecuteNonQuery(info.OracleParameterList, execute.ToString());

            ProcessOutParameters(info.Outputs);
        }

        private string PrepareStatement(string declare, string body, string query, string outparameters)
        {
            var execute = new StringBuilder();
            execute.AppendLine(declare.ToString());
            execute.AppendLine(body);
            execute.AppendLine(query);
            execute.AppendLine(outparameters.ToString());
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

            await Task.WhenAll(taskList.ToArray());
        }

        private void ProcessOutParameters(List<PreparedOutputParameter> outputs)
        {
            Parallel.ForEach(outputs, (param) =>
            {
                param.Parameter.SetOutputValue(param.OracleParameter.Value);
            });
        }

        private async Task ExecuteNonQueryAsync(List<OracleParameter> oracleParameterList, string execute)
        {
            if (_Connection.State != ConnectionState.Open)
            {
                await _Connection.OpenAsync();
            }

            var cmd = _Connection.CreateCommand();
            cmd.Parameters.AddRange(oracleParameterList.ToArray());
            cmd.CommandText = execute;

            await cmd.ExecuteNonQueryAsync();
        }

        private void ExecuteNonQuery(List<OracleParameter> oracleParameterList, string execute)
        {
            if (_Connection.State != ConnectionState.Open)
            {
                _Connection.Open();
            }

            var cmd = _Connection.CreateCommand();
            cmd.Parameters.AddRange(oracleParameterList.ToArray());
            cmd.CommandText = execute;

            cmd.ExecuteNonQuery();
        }

        private async Task LoadObjectParametersMetadataAsync(IParam[] parameters)
        {
            var tasksList = new List<Task>(parameters.Length);
            foreach (var param in parameters.Where(c => c is ParamObject).Cast<ParamObject>())
            {
                tasksList.Add(param.LoadObjectMetadataAsync(_Builder));
            }

            await Task.WhenAll(tasksList.ToArray());
        }

        private void LoadObjectParametersMetadata(IParam[] parameters)
        {
            Parallel.ForEach(parameters.Where(c => c is ParamObject).Cast<ParamObject>(), param =>
            {
                param.LoadObjectMetadata(_Builder);
            });
        }

        private StringBuilder ProcessDeclaration(IParam[] parameters)
        {
            var declaration = new StringBuilder();
            var objCounter = 0;

            declaration.AppendLine("declare");

            foreach (var param in parameters.Where(c => c is ParamManaged).Cast<ParamManaged>())
            {
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

            foreach (var param in parameters.Where(c => c is ParamObject).Cast<ParamObject>())
            {
                if (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput)
                {
                    var (Constructor, LastNumber) = param.BuildQueryConstructorString(info.ParameterCounter);
                    var oraParameters = param.GetOracleParameters(info.ParameterCounter);

                    info.OracleParameterList.AddRange(oraParameters);

                    body.AppendLine(Constructor);
                    info.ParameterCounter = LastNumber;
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
