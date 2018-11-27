using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Parameters
{
    public abstract class Param
    {
        protected internal Param(Type type, object value, ParameterDirection direction)
        {
            Type = type;
            Value = value;
            Direction = direction;
        }
        public ParameterDirection Direction { get; private set; }
        public virtual Type Type { get; }
        public virtual object Value { get; protected set; }
        internal abstract Task SetOutputValueAsync(object value);
        internal abstract void SetOutputValue(object value);

        public static void AddOracleUDTConfiguration(params (Type Type, OracleUDTInfo Info)[] udts)
        {
            MetadataBuilder.AddOracleUDTPresets(udts);
        }

        public static Param Create<T>(T value, ParameterDirection direction)
        {
            var type = typeof(T);
            if(type.IsValueType || type == typeof(string))
            {
                return new ParamCLRType<T>(value, direction);
            }
            else
            {
                return new ParamObject<T>(value, direction);
            }
        }

        public static Param Input<T>(T value) => Create(value, ParameterDirection.Input);
        public static Param Output<T>() => Create(default(T), ParameterDirection.Output);
        public static Param InputOutput<T>(T value) => Create(value, ParameterDirection.InputOutput);
    }
}
