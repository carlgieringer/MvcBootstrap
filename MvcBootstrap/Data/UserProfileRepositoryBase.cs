namespace MvcBootstrap.Data
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    using MvcBootstrap.Models;

    public abstract class UserProfileRepositoryBase<TUserProfile> : 
        BootstrapRepositoryBase<TUserProfile>,
        IUserProfileRepository<TUserProfile>
        where TUserProfile : class, IUserProfile, new()
    {
        protected UserProfileRepositoryBase(DbContext context)
            : base(context)
        {
        }

        public override void OnCreate(TUserProfile userProfile)
        {
            userProfile.LastLogin = DateTime.Now;
        }

        public TUserProfile GetByUsername(string username)
        {
            return this.Items.SingleOrDefault(up => up.Username.ToLower() == username.ToLower());
        }
    }
}