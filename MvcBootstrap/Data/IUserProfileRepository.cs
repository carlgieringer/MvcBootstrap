namespace MvcBootstrap.Data
{
    using MvcBootstrap.Models;

    public interface IUserProfileRepository<TUserProfile> : 
        IBootstrapRepository<TUserProfile>
        where TUserProfile : class, IUserProfile
    {
        TUserProfile GetByUsername(string username);
    }
}
