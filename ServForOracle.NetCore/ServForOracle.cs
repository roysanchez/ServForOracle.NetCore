using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.Metadata;

namespace ServForOracle.NetCore
{
    public class ServForOracle
    {
        private readonly OracleConnection _Connection;
        private readonly MetadataBuilder _Builder;
        private readonly ParamHandler _ParamHandler;

        public ServForOracle(OracleConnection connection)
        {
            _Connection = connection;
            _Builder = new MetadataBuilder(connection);
            _ParamHandler = new ParamHandler();
        }

        public void ExecuteProcedure(string procedure, params Param[] parameters)
        {
            Execute(procedure, parameters);
        }

        public T ExecuteFunction<T>(string function, params Param[] parameters)
        {
            return ExecuteFunction<T>(function, null, parameters);
        }

        public T ExecuteFunction<T>(string function, OracleUDTInfo udtInfo, params Param[] parameters)
        {
            var returnType = typeof(T);
            var returnMetadata = _Builder.GetOrRegisterMetadataOracleObject<T>(udtInfo);
            OracleParameter retOra = null;

            Execute($"ret := {function}", parameters, (info) =>
            {
                retOra = new OracleParameter($":{info.ParameterCounter}", DBNull.Value)
                {
                    OracleDbType = OracleDbType.RefCursor
                };
                info.OracleParameterList.Add(retOra);

                var returnInfo = new AdditionalInformation
                {
                    Declare = returnMetadata.GetDeclareLine(returnType, "ret", udtInfo ?? returnMetadata.OracleTypeNetMetadata.UDTInfo),
                    Output = returnMetadata.GetRefCursorQuery(info.ParameterCounter, "ret")
                };

                return returnInfo;
            });

            return (T)returnMetadata.GetValueFromRefCursor(returnType, retOra.Value as OracleRefCursor);
        }

        private static void ProcessOutParameters(List<PreparedOutputParameter> outputs)
        {
            foreach (var param in outputs)
            {
                param.Parameter.SetOutputValue(param.OracleParameter.Value);
            }
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

        private void Execute(string method, Param[] parameters, Func<ExecutionInformation, AdditionalInformation> beforeEnd = null)
        {
            LoadObjectParametersMetadata(parameters);

            var info = new ExecutionInformation();
            var (declare, body) = ProcessDeclarationAndBody(parameters, info);
            var query = ProcessQuery(method, parameters, info);
            var outparameters = ProcessOutputParameters(parameters, info);

            var additionalInfo = beforeEnd?.Invoke(info);
            if (additionalInfo != null)
            {
                declare.AppendLine(additionalInfo.Declare);
                outparameters.AppendLine(additionalInfo.Output);
            }

            var execute = new StringBuilder();
            execute.AppendLine(declare.ToString());
            execute.AppendLine(body);
            execute.AppendLine(query);
            execute.AppendLine(outparameters.ToString());
            execute.Append("end;");

            ExecuteNonQuery(info.OracleParameterList, execute.ToString());

            ProcessOutParameters(info.Outputs);
        }

        private void LoadObjectParametersMetadata(Param[] parameters)
        {
            foreach (ParamObject param in parameters.Where(c => c is ParamObject))
            {
                param.LoadObjectMetadata(_Builder);
            }
        }

        private (StringBuilder declaration, string body) ProcessDeclarationAndBody(Param[] parameters, ExecutionInformation info)
        {
            var body = new StringBuilder();
            var declaration = new StringBuilder();
            var objCounter = 0;

            declaration.AppendLine("declare");
            body.AppendLine("begin");

            foreach (ParamObject param in parameters
                .Where(c => c is ParamObject))
            {
                var name = $"p{objCounter++}";
                param.SetParameterName(name);

                declaration.AppendLine(param.GetDeclareLine());

                if (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput)
                {
                    var (Constructor, LastNumber) = param.BuildQueryConstructorString(name, info.ParameterCounter);
                    var oraParameters = param.GetOracleParameters(info.ParameterCounter);

                    info.OracleParameterList.AddRange(oraParameters);

                    body.AppendLine(Constructor);
                    info.ParameterCounter = LastNumber;
                }
            }

            return (declaration, body.ToString());
        }

        private StringBuilder ProcessOutputParameters(Param[] parameters, ExecutionInformation info)
        {
            var outparameters = new StringBuilder();
            foreach (ParamObject param in parameters
                .Where(c => c is ParamObject)
                .Where(c => c.Direction == ParameterDirection.Output || c.Direction == ParameterDirection.InputOutput))
            {
                var preparedOutput = param.PrepareOutputParameter(info.ParameterCounter);
                outparameters.AppendLine(preparedOutput.RefCursorString);

                info.OracleParameterList.Add(preparedOutput.OracleParameter);
                info.Outputs.Add(preparedOutput);
            }

            return outparameters;
        }

        private string ProcessQuery(string method, Param[] parameters, ExecutionInformation info)
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
                else if (param is ParamCLRType clrType)
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
