namespace MvcBootstrap.Models
{
    using System;
    using System.Data.Entity.ModelConfiguration;

    public class UserProfileBase : EntityBase, IUserProfile
    {
        public string Username { get; set; }

        public DateTime LastLogin { get; set; }
    }

    public class UserProfileConfiguration : EntityTypeConfiguration<UserProfileBase>
    {
        public UserProfileConfiguration()
        {
            //this.Property(p => p.Username).
        }
    }
}
