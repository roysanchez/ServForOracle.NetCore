using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.Metadata;

namespace ServForOracle.NetCore
{
    public class ServForOracle
    {
        private readonly OracleConnection _Connection;
        private readonly MetadataBuilder _Builder;

        public ServForOracle(OracleConnection connection)
        {
            _Connection = connection;
            _Builder = new MetadataBuilder(connection);
        }

        private readonly ParamHandler _ParamHandler = new ParamHandler();

        //private static readonly MethodInfo PrepareObject = typeof(ParamHandler).GetMethod(nameof(ParamHandler.PrepareParameterForQuery));
        //private static readonly MethodInfo OutputObject = typeof(ParamHandler).GetMethod(nameof(ParamHandler.PrepareOutputParameter));

        public T ExecuteFunction<T>(string function, params Param[] parameters)
        {
            return ExecuteFunction<T>(function, null, parameters);
        }

        public void ExecuteProcedure(string procedure, params Param[] parameters)
        {
            if (_Connection.State != ConnectionState.Open)
            {
                _Connection.Open();
            }

            foreach (ParamObject param in parameters.Where(c => c is ParamObject))
            {
                param.LoadObjectMetadata(_Builder);
            }

            var cmd = _Connection.CreateCommand();

            var declare = new StringBuilder();
            var query = new StringBuilder($"{procedure}(");
            var body = new StringBuilder();
            var outparameters = new StringBuilder();
            var outputs = new List<PreparedOutputParameter>();

            declare.AppendLine("declare");
            body.AppendLine("begin");

            var objCounter = 0;
            var counter = 0;
            bool first = true;
            foreach (ParamObject param in parameters
                .Where(c => c is ParamObject))
            {
                var name = $"p{objCounter++}";
                param.SetParameterName(name);

                declare.AppendLine(param.GetDeclareLine());

                if (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput)
                {
                    var (Constructor, LastNumber) = param.BuildQueryConstructorString(name, counter);
                    var oraParameters = param.GetOracleParameters(counter);

                    cmd.Parameters.AddRange(oraParameters);

                    body.AppendLine(Constructor);
                    counter = LastNumber;
                }
            }

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
                else if(param is ParamCLRType clr)
                {
                    var name = $":{counter++}";
                    query.Append(name);
                    var oracleParameter = clr.GetOracleParameter(name);
                    cmd.Parameters.Add(oracleParameter);
                    if (clr.Direction == ParameterDirection.Output || clr.Direction == ParameterDirection.InputOutput)
                    {
                        outputs.Add(new PreparedOutputParameter(clr, oracleParameter));
                    }
                }
            }

            foreach (ParamObject param in parameters
                .Where(c => c is ParamObject)
                .Where(c => c.Direction == ParameterDirection.Output || c.Direction == ParameterDirection.InputOutput))
            {
                var preparedOutput = param.PrepareOutputParameter(counter);
                outparameters.AppendLine(preparedOutput.RefCursorString);

                cmd.Parameters.Add(preparedOutput.OracleParameter);
                outputs.Add(preparedOutput);
            }

            var execute = new StringBuilder();
            execute.AppendLine(declare.ToString());
            execute.AppendLine(body.ToString());
            execute.Append(query.ToString());
            execute.AppendLine(");");
            execute.Append(outparameters.ToString());
            execute.Append("end;");

            cmd.CommandText = execute.ToString();

            cmd.ExecuteNonQuery();

            foreach (var param in outputs)
            {
                param.Parameter.SetOutputValue(param.OracleParameter.Value);
            }
        }

        public T ExecuteFunction<T>(string function, OracleUDTInfo udtInfo, params Param[] parameters)
        {
            if (_Connection.State != ConnectionState.Open)
            {
                _Connection.Open();
            }

            foreach (ParamObject param in parameters.Where(c => c is ParamObject))
            {
                param.LoadObjectMetadata(_Builder);
            }

            var cmd = _Connection.CreateCommand();

            var declare = new StringBuilder();
            var query = new StringBuilder($"ret := {function}(");
            var body = new StringBuilder();
            var outparameters = new StringBuilder();
            var outputs = new List<PreparedOutputParameter>();

            var returnType = typeof(T);

            var returnMetadata = _Builder.GetOrRegisterMetadataOracleObject<T>(udtInfo);
            if (udtInfo == null)
            {
                udtInfo = returnMetadata.OracleTypeNetMetadata.UDTInfo;
            }

            declare.AppendLine("declare");
            declare.AppendLine(returnMetadata.GetDeclareLine(returnType, "ret", udtInfo));

            body.AppendLine("begin");

            var objCounter = 0;
            var counter = 0;
            bool first = true;
            foreach (ParamObject param in parameters
                .Where(c => c is ParamObject))
            {
                var name = $"p{objCounter++}";
                param.SetParameterName(name);

                declare.AppendLine(param.GetDeclareLine());

                if (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput)
                {
                    var (Constructor, LastNumber) = param.BuildQueryConstructorString(name, counter);
                    var oraParameters = param.GetOracleParameters(counter);

                    cmd.Parameters.AddRange(oraParameters);

                    body.AppendLine(Constructor);
                    counter = LastNumber;
                }
            }

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
                else if(param is ParamCLRType clrType)
                {
                    var name = $":{counter++}";
                    query.Append(name);
                    var oracleParameter = clrType.GetOracleParameter(name);
                    cmd.Parameters.Add(oracleParameter);
                    if (clrType.Direction == ParameterDirection.Output || clrType.Direction == ParameterDirection.InputOutput)
                    {
                        outputs.Add(new PreparedOutputParameter(clrType, oracleParameter, null));
                    }
                }
            }

            foreach (ParamObject param in parameters
                .Where(c => c is ParamObject)
                .Where(c => c.Direction == ParameterDirection.Output || c.Direction == ParameterDirection.InputOutput))
            {
                var preparedOutput = param.PrepareOutputParameter(counter);
                outparameters.AppendLine(preparedOutput.RefCursorString);

                cmd.Parameters.Add(preparedOutput.OracleParameter);
                outputs.Add(preparedOutput);
            }

            var execute = new StringBuilder();
            execute.AppendLine(declare.ToString());
            execute.AppendLine(body.ToString());
            execute.Append(query.ToString());
            execute.AppendLine(");");
            execute.Append(outparameters.ToString());

            var retOra = new OracleParameter($":{counter}", DBNull.Value)
            {
                OracleDbType = OracleDbType.RefCursor
            };

            cmd.Parameters.Add(retOra);
            execute.AppendLine(returnMetadata.GetRefCursorCollectionQuery(counter, "ret"));

            execute.Append("end;");

            cmd.CommandText = execute.ToString();

            cmd.ExecuteNonQuery();

            foreach (var param in outputs)
            {
                param.Parameter.SetOutputValue(param.OracleParameter.Value);
            }

            return (T)returnMetadata.GetValueFromRefCursor(returnType, retOra.Value as OracleRefCursor);
        }
    }
}
