namespace MvcBootstrap.Data
{
    using System;

    public class ConcurrentUpdateException : Exception
    {
        public readonly object Entity;

        public ConcurrentUpdateException(object entity)
        {
            this.Entity = entity;
        }
    }
}