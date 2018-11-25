using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ServForOracle.NetCore
{
    public class RamoObj
    {
        public string CodRamo { get; set; }
        public string DescRamo { get; set; }
        public string TipoRamo { get; set; }
        //public string StsRamo { get; set; }
    }

    public class PreparedParameter
    {
        public PreparedParameter(string constructor, int startNumber, int lastNumber, IEnumerable<OracleParameter> parameters)
        {
            ConstructorString = constructor;
            StartNumber = startNumber;
            LastNumber = lastNumber;
            Parameters = parameters;
        }

        public string ConstructorString { get; private set; }
        public int StartNumber { get; private set; }
        public int LastNumber { get; private set; }
        public IEnumerable<OracleParameter> Parameters { get; private set; }
    }

    public class PreparedOutputParameter
    {
        public PreparedOutputParameter(Param parameter, OracleParameter oracleParameter, string refCursorString)
        {
            Parameter = parameter;
            OracleParameter = oracleParameter;
            RefCursorString = refCursorString;
        }

        public Param Parameter { get; private set; }
        public OracleParameter OracleParameter { get; private set; }
        public string RefCursorString { get; set; }
    }

    public class MetadataOracleTypeProperty
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public PropertyInfo NETProperty { get; set; }
    }

    public class MetadataOracleType
    {
        public IEnumerable<MetadataOracleTypeProperty> Properties { get; set; }
        public string Schema { get; set; }
        public string ObjectName { get; set; }
        public string CollectionName { get; set; }
    }

    public class MetadataOracleObject<T>: MetadataOracle where T : new()
    {
        private readonly Regex regex;
        private readonly string ConstructorString;

        private readonly MetadataOracleType metadata;
        private readonly Type Type = typeof(T);

        public MetadataOracleObject(string schema, string objectName, string listName, OracleConnection connection)
            : this(schema, objectName, connection)
        {
            metadata.CollectionName = listName;
        }

        public MetadataOracleObject(string schema, string objectName, OracleConnection connection)
        {
            regex = new Regex(Regex.Escape("$"));

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select attr_no, attr_name, attr_type_name from all_type_attrs where owner = "
                + "upper(:1) and type_name = upper(:2)";
            cmd.Parameters.Add(new OracleParameter(":1", schema));
            cmd.Parameters.Add(new OracleParameter(":2", objectName));

            var reader = cmd.ExecuteReader();
            var properties = new List<MetadataOracleTypeProperty>();
            var NETProperties = Type.GetProperties();

            while (reader.Read())
            {
                var property = new MetadataOracleTypeProperty
                {
                    Order = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };

                //TODO Buscar por el atributo
                property.NETProperty = NETProperties
                    .Where(c => c.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();

                properties.Add(property);
            }

            metadata = new MetadataOracleType { Properties = properties, ObjectName = objectName, Schema = schema };

            var constructor = new StringBuilder($"{schema}.{objectName}(");

            for (var counter = 0; counter < properties.Count; counter++)
            {
                constructor.Append(properties[counter].Name);
                constructor.Append("=>");

                constructor.Append('$');
                if (counter + 1 < properties.Count)
                {
                    constructor.Append(',');
                }
            }
            constructor.Append(");");

            ConstructorString = constructor.ToString();
        }

        public (string Constructor, int LastNumber) BuildConstructor(T value, int startNumber)
        {
            var constructor = ConstructorString;
            foreach (var prop in metadata.Properties.OrderBy(c => c.Order))
            {
                if (prop.NETProperty != null)
                {
                    constructor = regex.Replace(constructor, $":{startNumber++}", 1);
                }
                else
                {
                    constructor = regex.Replace(constructor, "null", 1);
                }

            }

            return (constructor, startNumber);
        }

        public OracleParameter[] GetOracleParameters(T value, int startNumber)
        {
            var parameters = new List<OracleParameter>();
            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
            {
                parameters.Add(new OracleParameter($":{startNumber++}", prop.NETProperty.GetValue(value)));
            }

            return parameters.ToArray();
        }

        public string GetRefCursorCollectionQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber++} for select ");
            var first = true;
            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(",");
                }
                query.Append($"c.{prop.Name}");
            }
            query.Append($" from table({fieldName}) c;");

            return query.ToString();
        }

        public string GetRefCursorQuery(int startNumber, string fieldName)
        {
            var query = new StringBuilder($"open :{startNumber++} for select ");
            var first = true;
            foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(",");
                }
                query.Append($"{fieldName}.{prop.Name}");
            }
            query.Append(" from dual;");

            return query.ToString();
        }

        public OracleParameter GetOracleParameterForRefCursor(int starNumber)
        {
            return new OracleParameter($":{starNumber}", DBNull.Value)
            {
                OracleDbType = OracleDbType.RefCursor
            };
        }

        public T GetValueFromRefCursor(OracleRefCursor refCursor)
        {
            var instance = new T();

            var reader = refCursor.GetDataReader();
            while (reader.Read())
            {
                var count = 0;
                foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                {
                    prop.NETProperty.SetValue(instance,
                        ConvertOracleParameterToBaseType(prop.NETProperty.PropertyType, reader.GetOracleValue(count++)));
                }
            }

            return instance;
        }

        public T[] GetListValueFromRefCursor(OracleRefCursor refCursor)
        {
            var list = new List<T>();

            var reader = refCursor.GetDataReader();
            while (reader.Read())
            {
                var count = 0;
                var instance = new T();
                foreach (var prop in metadata.Properties.Where(c => c.NETProperty != null).OrderBy(c => c.Order))
                {
                    prop.NETProperty.SetValue(instance,
                        ConvertOracleParameterToBaseType(prop.NETProperty.PropertyType, reader.GetOracleValue(count++)));
                }
                list.Add(instance);
            }

            return list.ToArray();
        }
    }

    public class MetadataOracle
    {
        public object ConvertOracleParameterToBaseType(Type retType, object oracleParam)
        {
            bool isNullable = (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Nullable<>));

            object value = null;

            if (retType == oracleParam.GetType())
            {
                return oracleParam;
            }
            
            switch (oracleParam)
            {
                case DBNull nulo:
                    break;
                case OracleDecimal dec:
                    if (dec.IsNull)
                    {
                        if (isNullable || !retType.IsValueType || retType == typeof(string))
                            value = null;
                        else
                            throw new InvalidCastException($"Can't cast a null value to {retType.Name}");
                    }
                    else if (retType == typeof(int) || retType == typeof(int?))
                        value = dec.ToInt32();
                    else if (retType == typeof(float) || retType == typeof(float?) || retType == typeof(Single))
                        value = dec.ToSingle();
                    else if (retType == typeof(double) || retType == typeof(double?))
                        value = dec.ToDouble();
                    else if (retType == typeof(decimal) || retType == typeof(decimal?))
                        value = dec.Value;
                    else if (retType == typeof(byte) || retType == typeof(byte?))
                        value = dec.ToByte();
                    else if (retType == typeof(string))
                        value = dec.ToString();
                    else
                        throw new InvalidCastException($"Can't cast OracleDecimal to {retType.Name}");
                    break;
                case OracleString str when retType == typeof(string):
                    if (str.IsNull)
                        value = null;
                    else
                        value = str.ToString();
                    break;
                //case OracleClob clob when retType == typeof(string):
                //    value = ExtractValue(clob);
                //    break;
                //case OracleBFile file when retType == typeof(byte[]):
                //    value = ExtractValue(file);
                //    break;
                //case OracleBlob blob when retType == typeof(byte[]):
                //    value = ExtractValue(blob);
                //    break;
                //case OracleDate date when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    value = ExtractNullableValue(date, isNullable);
                //    break;
                //case OracleIntervalDS interval when retType == typeof(TimeSpan) || retType == typeof(TimeSpan?):
                //    value = ExtractNullableValue(interval, isNullable);
                //    break;
                //case OracleIntervalYM intervalYM when (
                //        retType == typeof(long) || retType == typeof(long?) ||
                //        retType == typeof(float) || retType == typeof(float?) ||
                //        retType == typeof(double) || retType == typeof(double?)
                //    ):
                //    value = ExtractNullableValue(intervalYM, isNullable);
                //    break;
                //case OracleBinary binary when retType == typeof(byte[]):
                //    value = ExtractValue(binary);
                //    break;
                //case OracleRef reff when retType == typeof(string):
                //    value = ExtractValue(reff);
                //    break;
                //case OracleTimeStamp timestamp when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    ExtractNullableValue(timestamp, isNullable);
                //    break;
                //case OracleTimeStampLTZ timestampLTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    ExtractNullableValue(timestampLTZ, isNullable);
                //    break;
                //case OracleTimeStampTZ timestampTZ when retType == typeof(DateTime) || retType == typeof(DateTime?):
                //    ExtractNullableValue(timestampTZ, isNullable);
                //    break;
                default:
                    break;
            }

            return value;
        }
    }

    public class Param<T> : Param where T : new()
    {
        public MetadataOracle Metadata { get; private set; }
        public new T Value { get; private set; }
        public override bool IsOracleType { get => false; }

        public Param(T value, ParameterDirection direction)
            : base(value, direction)
        {
            Metadata = new MetadataOracle();
            Value = value;
        }

        internal override void SetOutputValue(object value)
        {
            Value = (T)Metadata.ConvertOracleParameterToBaseType(typeof(T), value);
        }
    }

    public class ParamObject<T> : ParamObject where T : new()
    {
        public MetadataOracleObject<T> Metadata { get; private set; }

        public new T Value { get; private set; }

        private readonly string _Schema;
        private readonly string _ObjectName;
        private readonly string _ListName;
        private string _ParameterName;

        public override bool IsOracleType => true;
        public override string Schema => _Schema;
        public override string ObjectName => base.ObjectName;
        public override Type Type => typeof(T);
        public override string ParameterName => _ParameterName;

        public ParamObject(T value, string schema, string objectName, OracleConnection con,
            ParameterDirection direction, string listName = null)
            : base(value, direction)
        {
            _Schema = schema;
            _ObjectName = objectName;
            _ListName = listName;
            Metadata = new MetadataOracleObject<T>(schema, objectName, con);
            Value = value;
        }

        public override void SetParameterName(string name)
        {
            _ParameterName = name;
        }

        public override string GetDeclareLine()
        {
            return $"{_ParameterName} {_Schema}.{_ObjectName};";
        }

        internal override void SetOutputValue(object value)
        {
            Value = Metadata.GetValueFromRefCursor(value as OracleRefCursor);
        }
    }

    public abstract class ParamObject : Param
    {
        public ParamObject(object value, ParameterDirection direction)
            : base(value, direction)
        {

        }

        public virtual string ParameterName { get; }
        public virtual string Schema { get; }
        public virtual string ObjectName { get; }
        public abstract void SetParameterName(string name);
        public abstract string GetDeclareLine();
    }

    public abstract class Param
    {
        public ParameterDirection Direction { get; private set; }
        public Param(object value, ParameterDirection direction)
        {
            Value = value;
            Direction = direction;
        }
        public virtual Type Type { get; }
        public virtual object Value { get; protected set; }
        public virtual bool IsOracleType { get; }
        internal abstract void SetOutputValue(object value);

    }

    public class ParamHandler
    {
        //public (string, int) ParameterBodyConstruction<T>(string name, Param<T> parameter, int startNumber)
        //    where T : new()
        //{
        //    var constructor = parameter.Metadata.BuildConstructor(parameter.Value, startNumber);
        //    return (name + " := " + constructor.Constructor, constructor.LastNumber);
        //}

        public PreparedParameter PrepareParameterForQuery<T>(string name, ParamObject<T> parameter, int startNumber)
            where T : new()
        {
            var (constructor, lastNumber) = parameter.Metadata.BuildConstructor(parameter.Value, startNumber);
            var oracleParameters = parameter.Metadata.GetOracleParameters(parameter.Value, startNumber);

            return new PreparedParameter(constructor, startNumber, lastNumber, oracleParameters);
        }

        public PreparedOutputParameter PrepareOutputParameter<T>(string name, ParamObject<T> parameter, int startNumber)
            where T : new()
        {
            string query = string.Empty;
            if (typeof(T).IsArray)
            {
                query = parameter.Metadata.GetRefCursorCollectionQuery(startNumber, name);
            }
            else
            {
                query = parameter.Metadata.GetRefCursorQuery(startNumber, name);
            }

            var oracleParameter = parameter.Metadata.GetOracleParameterForRefCursor(startNumber);

            return new PreparedOutputParameter(parameter, oracleParameter, query);
        }
    }


    public class ServForOracle
    {
        private readonly ParamHandler _ParamHandler = new ParamHandler();

        //private static readonly MethodInfo ParamBody = typeof(ParamHandler).GetMethod(nameof(ParamHandler.ParameterBodyConstruction));
        private static readonly MethodInfo PrepareObject = typeof(ParamHandler).GetMethod(nameof(ParamHandler.PrepareParameterForQuery));
        private static readonly MethodInfo OutputObj = typeof(ParamHandler).GetMethod(nameof(ParamHandler.PrepareOutputParameter));

        public T[] ExecuteFunction<T>(string function, string schema, string obj, string list, OracleConnection con,
          params Param[] parameters) where T : new()
        {
            var cmd = con.CreateCommand();

            var declare = new StringBuilder();
            var query = new StringBuilder($"ret := {function}(");
            var body = new StringBuilder();
            var outparameters = new StringBuilder();
            var outputs = new List<PreparedOutputParameter>();

            var returnMetadata = new MetadataOracleObject<T>(schema, obj, list, con);//new ParamObject<T>(default, schema, obj, con);
            //returnMetadata.SetParameterName("ret");


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

                        body.AppendLine($"{name} := {preparedParameter.ConstructorString}");
                        counter = preparedParameter.LastNumber;
                    }

                    if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                    {
                        var outputMethod = OutputObj.MakeGenericMethod(param.Type);
                        var preparedOutput = outputMethod.Invoke(_ParamHandler, new object[] { name, param, counter++ })
                            as PreparedOutputParameter;
                        outparameters.AppendLine(preparedOutput.RefCursorString);

                        cmd.Parameters.Add(preparedOutput.OracleParameter);
                        outputs.Add(preparedOutput);
                    }

                    //TODO commas
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
                    //if (param.Direction == ParameterDirection.Input)
                    //{
                    //    cmd.Parameters.Add(new OracleParameter($":{counter++}", param.Value));
                    //}
                    //else
                    //{
                    //    var name = $"p{objCounter++}";
                    //    var oracleParameter = new OracleParameter(name, param.Value)
                    //    {
                    //        Direction = param.Direction
                    //    };
                    //    outputs.Add(new PreparedOutputParameter(param, oracleParameter, null));

                    //}
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

    public class Program
    {
        static void Main(string[] args)
        {
            var con = new OracleConnection("");
            con.Open();

            var serv = new ServForOracle();
            var ramon = new RamoObj() { CodRamo = "BABB" };
            var x = serv.ExecuteFunction<RamoObj>("uniserv.prueba_net_core_list_param", "UNISERV", "RAMO_OBJ", "RAMO_LIST", con,
                new ParamObject<RamoObj>(ramon, "UNISERV", "RAMO_OBJ", con, ParameterDirection.Input),
                new Param<DateTime>(DateTime.Now, ParameterDirection.Input));

            var zz = new MetadataOracleObject<RamoObj>("uniserv", "ramo_obj", con);



            var cmd = con.CreateCommand();

            var xx = zz.BuildConstructor(ramon, 0).Constructor;
            var props = zz.GetOracleParameters(ramon, 0);
            cmd.CommandText = $@"
            declare
                ret uniserv.ramo_list := uniserv.ramo_list();
            begin
                ret.extend;
                ret(ret.last) := {xx}
                uniserv.prueba_net_core_proc_list(ret);
            end;";
            cmd.Parameters.AddRange(props.ToArray());
            cmd.ExecuteNonQuery();



            //var yy = zz.GetRefCursorCollectionQuery(0, "ret");
            //var param = zz.GetOracleParameterForRefCursor(0);
            //cmd.CommandText = $@"
            //declare
            //    ret uniserv.ramo_list := uniserv.ramo_list();
            //begin
            //    ret := uniserv.prueba_net_core_list();
            //    {yy}
            //end;";

            //cmd.Parameters.Add(param);
            //cmd.ExecuteNonQuery();
            //var valList = zz.GetListValueFromRefCursor(param.Value as OracleRefCursor);

            //var xx = zz.BuildConstructor(ramon, 0).Constructor;
            //var props = zz.GetOracleParameters(ramon, 0);
            //cmd.CommandText = $@"
            //declare
            //    ret uniserv.ramo_obj;
            //begin
            //    ret := {xx}
            //    uniserv.prueba_net_core_proc(ret);
            //end;";
            //cmd.Parameters.AddRange(props.ToArray());
            //cmd.ExecuteNonQuery();


            //var yy = zz.GetRefCursorQuery(ramon, 0, "ret");
            //var param = zz.GetOracleParameterForRefCursor(0);

            //cmd.CommandText = $@"
            //declare
            //    ret uniserv.ramo_obj;
            //begin
            //    ret := uniserv.prueba_net_core();
            //    {yy}
            //   -- open :1 for
            //   --     select ret.codramo, ret.descramo, ret.tiporamo, ret.stsramo from dual;
            //end;";
            //cmd.Parameters.Add(param);

            //cmd.ExecuteNonQuery();

            //var valor = zz.GetValueFromRefCursor(param.Value as OracleRefCursor);


            //var param = cmd.CreateParameter();
            ////param.Value = 1;
            //param.OracleDbType = OracleDbType.RefCursor;

            //cmd.Parameters.Add(param);

            ////var reader = cmd.ExecuteReader();
            //cmd.ExecuteNonQuery();

            //var refCursor = param.Value as OracleRefCursor;
            //var reader = refCursor.GetDataReader();

            //while (reader.Read())
            //{
            //    var x = reader.GetOracleString(0);
            //}

            Console.WriteLine("Hello World!");
        }
    }
}
