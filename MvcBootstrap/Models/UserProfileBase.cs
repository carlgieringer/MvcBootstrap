namespace MvcBootstrap.Models
{
    using System;
    using System.Data.Entity.ModelConfiguration;

    public class UserProfileBase : EntityBase, IUserProfile
    {
        public virtual string Username { get; set; }

        public virtual DateTime LastLogin { get; set; }
    }

    public class UserProfileConfiguration : EntityTypeConfiguration<UserProfileBase>
    {
        public UserProfileConfiguration()
        {
            //this.Property(p => p.Username).
        }
    }
}
