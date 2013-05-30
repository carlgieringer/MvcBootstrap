namespace MvcBootstrap.ViewModels.Accounts
{
    using MvcBootstrap.Models;

    /// <summary>
    /// Adds <see cref="ExternalLoginRegistrationModel{TUserProfileViewModel}.ExternalLoginData"/>
    /// to the <see cref="TUserProfileViewModel"/> so that user profile information can be requested 
    /// from a user logging in with a third-party authenticator for the first time and 
    /// reconnect the information to the login information to create a new <see cref="TUserProfileViewModel"/>.
    /// </summary>
    /// <typeparam name="TUserProfileViewModel"></typeparam>
    public class ExternalLoginRegistrationModel<TUserProfileViewModel> : IExternalLoginRegistrationModel
    {
        public TUserProfileViewModel UserProfileViewModel { get; set; }

        public string ExternalLoginData { get; set; }
    }
}