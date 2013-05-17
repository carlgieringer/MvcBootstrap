namespace MvcBootstrap.Exceptions
{
    using System;

    public class MvcBootstrapException : Exception
    {
        public MvcBootstrapException()
        {
        }

        public MvcBootstrapException(string message)
            : base(message)
        {
        }

        public MvcBootstrapException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
