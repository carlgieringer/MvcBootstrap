namespace MvcBootstrap.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration;

    public abstract class EntityBase : IEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; protected set; }

        public virtual DateTime Created { get; set; }

        public virtual DateTime Modified { get; set; }

        [Timestamp, ConcurrencyCheck]
        public virtual byte[] Timestamp { get; set; }
    }

    public class EntityBaseConfiguration : EntityTypeConfiguration<EntityBase>
    {
        public EntityBaseConfiguration()
        {
            this.HasKey(e => e.Id);
            this.Property(e => e.Timestamp).IsConcurrencyToken();
        }
    }
}