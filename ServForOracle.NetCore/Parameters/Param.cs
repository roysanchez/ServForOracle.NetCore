using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Parameters
{
    public interface IParam<T> : IParam
    {
        T Value { get; }
    }

    public interface IParam
    {
        Type Type { get; }
        ParameterDirection Direction { get; }
    }

    public abstract class Param: IParam
    {
        protected Param(Type type, object value, ParameterDirection direction)
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

        public static IParam<T> Create<T>(T value, ParameterDirection direction)
        {
            var type = typeof(T);
            if (type.IsValueType || type == typeof(string))
            {
                return new ParamClrType<T>(value, direction);
            }
            else
            {
                return new ParamObject<T>(value, direction);
            }
        }

        public static IParam<T> Input<T>(T value) => Create(value, ParameterDirection.Input);
        public static IParam<T> Output<T>() => Create(default(T), ParameterDirection.Output);
        public static IParam<T> InputOutput<T>(T value) => Create(value, ParameterDirection.InputOutput);
    }
}
