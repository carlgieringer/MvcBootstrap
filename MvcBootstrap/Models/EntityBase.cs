namespace MvcBootstrap.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public abstract class EntityBase : IEntity
    {
        [Key]
        public virtual int Id { get; protected set; }

        public virtual DateTime Created { get; set; }

        public virtual DateTime Modified { get; set; }

        [Timestamp, ConcurrencyCheck]
        public virtual byte[] Timestamp { get; set; }
    }
}