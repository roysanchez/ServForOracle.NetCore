using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ServForOracle.NetCore.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsBoolean(this Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        public static bool IsClrType(this Type type)
        {
            return type != null && (
                type == typeof(uint) ||
                type == typeof(uint?) ||
                type == typeof(ulong) ||
                type == typeof(ulong?) ||
                type == typeof(byte) ||
                type == typeof(byte?) ||
                type == typeof(short) ||
                type == typeof(short?) ||
                type == typeof(int) ||
                type == typeof(int?) ||
                type == typeof(long) ||
                type == typeof(long?) ||
                type == typeof(bool) ||
                type == typeof(bool?) ||
                type == typeof(char) ||
                type == typeof(char?) ||
                type == typeof(float) ||
                type == typeof(float?) ||
                type == typeof(double) ||
                type == typeof(double?) ||
                type == typeof(decimal) ||
                type == typeof(decimal?) ||
                type == typeof(DateTime) ||
                type == typeof(DateTime?) ||
                type == typeof(TimeSpan) ||
                type == typeof(TimeSpan?) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(DateTimeOffset?) ||
                type == typeof(Guid) ||
                type == typeof(Guid?) ||
                type == typeof(string)
            );
        }

        public static bool CanMapToOracle(this Type type)
        {
            return type != null && (
                type == typeof(string) ||
                type == typeof(char) ||
                type == typeof(char?) ||
                type == typeof(short) ||
                type == typeof(short?) ||
                type == typeof(byte) ||
                type == typeof(byte?) ||
                type == typeof(int) ||
                type == typeof(int?) ||
                type == typeof(long) ||
                type == typeof(long?) ||
                type == typeof(float) ||
                type == typeof(float?) ||
                type == typeof(double) ||
                type == typeof(double?) ||
                type == typeof(decimal) ||
                type == typeof(decimal?) ||
                type == typeof(DateTime) ||
                type == typeof(DateTime?) ||
                type == typeof(bool) ||
                type == typeof(bool?)
            );
        }

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


        public static object CreateInstance(this Type type, params object[] arguments)
        {
            //TODO Add support for nullable types
            if (type.IsCollection())
            {
                if (type.IsArray)
                {
                    return Enumerable.ToArray((dynamic)Activator.CreateInstance(type.GetCollectionUnderType().CreateListType(), arguments));
                }
                else
                {
                    return Activator.CreateInstance(type.GetCollectionUnderType().CreateListType(), arguments);
                }
            }
            else return Activator.CreateInstance(type, arguments);
        }

        public static Type CreateListType(this Type underType)
        {
            return typeof(List<>).MakeGenericType(underType);
        }
    }
}
