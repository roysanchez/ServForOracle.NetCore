using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ServForOracle.NetCore.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsCollection(this Type type)
        {
            return
                type.IsArray
                ||
                (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ||
                (type != typeof(string) && !type.IsValueType && type.GetInterface(nameof(IEnumerable)) != null);
        }

        public static Type GetCollectionUnderType(this Type type)
        {
            if (type == null || !IsCollection(type))
                return null;

            var ga = type.GetGenericArguments();
            if (ga.Length > 0)
                return ga.Single();
            else
                return type.GetElementType();
        }


        public static object CreateInstance(this Type proxyType, params object[] arguments)
        {
            if (proxyType.IsCollection())
            {
                return Activator.CreateInstance(proxyType.GetCollectionUnderType().CreateListType(), arguments);
            }
            else return Activator.CreateInstance(proxyType, arguments);
        }

        public static Type CreateListType(this Type proxyUnderType)
        {
            return typeof(List<>).MakeGenericType(proxyUnderType);
        }
    }
}
