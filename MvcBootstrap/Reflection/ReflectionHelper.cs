
namespace MvcBootstrap.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using MvcBootstrap.Extensions;

    public static class ReflectionHelper
    {
        public static void SetProperty<T>(T obj, string propertyName, object value)
        {
            var pi = GetPropertyInfo(obj, propertyName);
            var setMethod = pi.GetSetMethod(true);
            if (setMethod != null)
            {
                setMethod.Invoke(obj, new[] { value });
            }
            else
            {
                throw new InvalidOperationException(@"Type ""{0}"" has no setter for ""{1}""".F(typeof(T), propertyName));
            }
        }

        public static PropertyInfo GetPropertyInfo<T>(T obj, string propertyName)
        {
            Type type;
            if (obj != null)
            {
                type = obj.GetType();
            }
            else
            {
                type = typeof(T);
            }

            //Type type = typeof(T);
            try
            {
                return type.GetProperty(propertyName);
            }
            catch (Exception)
            {
                throw new InvalidOperationException(@"Type ""{0}"" has no property ""{1}""".F(type, propertyName));
            }
        }

        public static object GetProperty<T>(T obj, string propertyName)
        {
            var pi = GetPropertyInfo<T>(obj, propertyName);
            return pi.GetValue(obj);
        }

        public static Type ExtractGenericInterface(Type type, Type targetInterfaceType)
        {
            if (MatchesGenericType(type, targetInterfaceType))
            {
                return type;
            }

            var typeInterfaces = type.GetInterfaces();
            return typeInterfaces.FirstOrDefault(interfaceType => MatchesGenericType(interfaceType, targetInterfaceType));
        }

        private static bool MatchesGenericType(Type type, Type matchType)
        {
            return 
                type.IsGenericType && 
                type.GetGenericTypeDefinition() == matchType;
        }
    }
}