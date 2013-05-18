namespace MvcBootstrap.Exceptions
{
    using System;

    using MvcBootstrap.Extensions;

    public class UnhandledEnumException<T> : Exception
    {
        public UnhandledEnumException(T value)
            : base("The {0} value {1} was not handled.".F(typeof(T).Name, value))
        {
            // Nada
        }
    }
}