namespace MvcBootstrap.Data
{
    using System;

    using MvcBootstrap.Exceptions;

    public class MvcBootstrapDataException : MvcBootstrapException
    {
        public MvcBootstrapDataException()
        {
        }

        public MvcBootstrapDataException(string message)
            :base(message)
        {
            // Nothing
        }

        public MvcBootstrapDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}