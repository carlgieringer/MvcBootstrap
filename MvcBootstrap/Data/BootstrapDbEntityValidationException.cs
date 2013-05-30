namespace MvcBootstrap.Data
{
    using System;
    using System.Data.Entity.Validation;

    /// <summary>
    /// A wrapper for <see cref="DbEntityValidationException"/> with an actually useful message about the 
    /// validation errors.
    /// </summary>
    public class BootstrapDbEntityValidationException : Exception
    {
        public BootstrapDbEntityValidationException(string message, DbEntityValidationException inner)
            : base(message, inner)
        {
        }
    }
}