namespace MvcBootstrap.Extensions
{
    using System;
    using System.Linq;

    using MvcBootstrap.Util;

    public static class TypeExtensions
    {
        public static string Description(this Type type)
        {
            return StringHelper.SplitCamelCase(type.Name);
        }

        public static bool IsAssignableTo(this Type type, Type other)
        {
            return other.IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines whether <paramref name="type"/> is a constructed generic type based upon
        /// <see cref="genericTypeDefinition"/> with type parameter <see cref="genericTypeParameter"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericTypeDefinition"></param>
        /// <param name="genericTypeArguments"></param>
        /// <returns></returns>
        public static bool IsConstructedGenericTypeOfDefinitionWith(this Type type, Type genericTypeDefinition, params Type[] genericTypeArguments)
        {
            return type.IsConstructedGenericTypeOfDefinition(genericTypeDefinition) &&
                type.GetGenericArguments()
                    .Zip(genericTypeArguments, (TypeArgument, TestTypeArgument) => new { TypeArgument, TestTypeArgument })
                    .All(x => x.TypeArgument.IsAssignableTo(x.TestTypeArgument));
        }

        public static bool IsConstructedGenericTypeOfDefinition(this Type type, Type genericTypeDefinition)
        {
            if (!genericTypeDefinition.ContainsGenericParameters)
            {
                throw new ArgumentException("Parameter must be a generic type with unbound type parameters", "genericTypeDefinition");
            }

            if (genericTypeDefinition.IsInterface)
            {
                if (type.IsInterface && 
                    type.IsGenericType && 
                    type.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    return true;
                }
                return type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == genericTypeDefinition);
            }
            else
            {
                return TypeIsConstructionOfClassGenericType(type, genericTypeDefinition);
            }
        }

        private static bool TypeIsConstructionOfClassGenericType(Type type, Type genericTypeDefinition)
        {
            Type toCheck = type;
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (cur == genericTypeDefinition)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}
