namespace MvcBootstrap.ViewModels.Accounts
{
    using System;

    public interface IUserProfileViewModel : IEntityViewModel
    {
        string Username { get; set; }

        DateTime? LastLogin { get; set; }
    }
}