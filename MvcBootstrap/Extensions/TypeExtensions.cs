namespace MvcBootstrap.Extensions
{
    using System;

    public static class TypeExtensions
    {
        public static string Description(this Type type)
        {
            return type.Name;
        }

        public static bool IsAssignableTo(this Type type, Type other)
        {
            return other.IsAssignableFrom(type);
        }
    }
}
