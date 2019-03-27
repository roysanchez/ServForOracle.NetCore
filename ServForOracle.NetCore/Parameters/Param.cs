using Oracle.ManagedDataAccess.Client;
using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServForOracle.NetCore.Parameters
{
    public abstract class Param: IParam
    {
        protected Param(Type type, object value, ParameterDirection direction)
        {
            Type = type;
            Direction = direction;
            Value = value;
        }
        public virtual ParameterDirection Direction { get; private set; }
        public virtual Type Type { get; }
        public virtual object Value { get; protected set; }
        internal abstract Task SetOutputValueAsync(object value);
        internal abstract void SetOutputValue(object value);

        public static IParam<T> Create<T>(T value, ParameterDirection direction)
        {
            var type = typeof(T);
            if (type.IsBoolean())
            {
                return new ParamBoolean(value as bool?, direction) as IParam<T>;
                
            }
            else if(type.IsClrType())
            {
                if (type.CanMapToOracle())
                {
                    return new ParamClrType<T>(value, direction);
                }
                else
                {
                    throw new ArgumentException($"The type {type.FullName} is not supported by the library as a direct map to oracle.");
                }
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
