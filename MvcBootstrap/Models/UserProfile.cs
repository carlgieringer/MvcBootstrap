namespace MvcBootstrap.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration;

    public class UserProfile : EntityBase, IUserProfile
    {
        [Required, MaxLength(128)]
        public virtual string Username { get; set; }

        [DataType(DataType.EmailAddress), Required]
        public virtual string Email { get; set; }

        public virtual DateTime? LastLogin { get; set; }
    }

    public class UserProfileConfiguration : EntityTypeConfiguration<UserProfile>
    {
        public UserProfileConfiguration()
        {
            //this.Property(p => p.Username).
        }
    }
}
