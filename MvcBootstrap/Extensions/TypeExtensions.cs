namespace MvcBootstrap.Extensions
{
    using System;
    using System.Linq;

    using MvcBootstrap.Reflection;

    public static class TypeExtensions
    {
        public static string Description(this Type type)
        {
            return StringHelper.InsertSpacesBetweenCamelCaseWords(type.Name);
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
        /// <param name="genericTypeParameter"></param>
        /// <returns></returns>
        public static bool IsConstructedGenericTypeFor(this Type type, Type genericTypeDefinition, Type genericTypeParameter)
        {
            return type.IsGenericType &&
                type.GetGenericTypeDefinition() == genericTypeDefinition &&
                type.GetGenericArguments().First().IsAssignableTo(genericTypeParameter);
        }

        public static bool IsConstructedGenericTypeFor(this Type type, Type genericTypeDefinition)
        {
            return type.IsGenericType &&
                type.GetGenericTypeDefinition() == genericTypeDefinition;
        }
    }
}
