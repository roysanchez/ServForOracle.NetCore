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
        
        private static readonly MethodInfo PrepareObject = typeof(ParamHandler).GetMethod(nameof(ParamHandler.PrepareParameterForQuery));
        private static readonly MethodInfo OutputObject = typeof(ParamHandler).GetMethod(nameof(ParamHandler.PrepareOutputParameter));

        public T[] ExecuteFunction<T>(string function, string schema = null, string obj = null, string list = null, params Param[] parameters)
        {
            if(_Connection.State != ConnectionState.Open)
            {
                _Connection.Open();
            }

            foreach(ParamObject param in parameters.Where(c => c is ParamObject))
            {
                param.LoadObjectMetadata(_Builder);
            }

            var cmd = _Connection.CreateCommand();

            var declare = new StringBuilder();
            var query = new StringBuilder($"ret := {function}(");
            var body = new StringBuilder();
            var outparameters = new StringBuilder();
            var outputs = new List<PreparedOutputParameter>();

            var returnMetadata = _Builder.GetOrRegisterMetadataOracleObject<T>(schema, obj, list);

            declare.AppendLine("declare");
            declare.AppendLine($"ret {schema}.{list} := {schema}.{list}();");

            body.AppendLine("begin");

            var objCounter = 0;
            var counter = 0;
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
                if (param.IsOracleType && param is ParamObject paramObject)
                {
                    var name = $"p{objCounter++}";
                    paramObject.SetParameterName(name);

                    declare.AppendLine(paramObject.GetDeclareLine());

                    if (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput)
                    {
                        var genericMethod = PrepareObject.MakeGenericMethod(param.Type);
                        var preparedParameter = genericMethod.Invoke(_ParamHandler, new object[] { name, param, counter })
                            as PreparedParameter;

                        cmd.Parameters.AddRange(preparedParameter.Parameters.ToArray());

                        if (param.Type.IsCollection())
                        {
                            body.AppendLine($"{name}.extend;");
                            body.AppendLine($"{name}({name}.last) := {preparedParameter.ConstructorString}");
                        }
                        else
                        {
                            body.AppendLine($"{name} := {preparedParameter.ConstructorString}");
                        }

                        counter = preparedParameter.LastNumber;
                    }

                    if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                    {
                        var outputMethod = OutputObject.MakeGenericMethod(param.Type);
                        var preparedOutput = outputMethod.Invoke(_ParamHandler, new object[] { name, param, counter++ })
                            as PreparedOutputParameter;
                        outparameters.AppendLine(preparedOutput.RefCursorString);

                        cmd.Parameters.Add(preparedOutput.OracleParameter);
                        outputs.Add(preparedOutput);
                    }
                    
                    query.Append(name);
                }
                else
                {
                    var name = $":{counter++}";
                    query.Append(name);
                    var oracleParameter = new OracleParameter(name, param.Value)
                    {
                        Direction = param.Direction
                    };
                    cmd.Parameters.Add(oracleParameter);
                    if(param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                    {
                        outputs.Add(new PreparedOutputParameter(param, oracleParameter, null));
                    }
                }
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

            var zz = returnMetadata.GetListValueFromRefCursor(retOra.Value as OracleRefCursor);

            foreach (var param in outputs)
            {
                param.Parameter.SetOutputValue(param.OracleParameter.Value);
            }

            return zz;
        }   
    }
}
