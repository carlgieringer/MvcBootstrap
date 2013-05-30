namespace MvcBootstrap.ViewModels.Accounts
{
    using MvcBootstrap.Models;
    using MvcBootstrap.Web.Mvc.Controllers;

    /// <summary>
    /// Users can implement this interface (directly, or via <see cref="ExternalLoginRegistrationModelBase"/>
    /// and use the concrete implementation type as a type parameter to 
    /// <see cref="AccountsControllerBase{TUserProfile,TExternalLoginRegistrationModel}"/>, and that controller will
    /// use an instance of that type as model to the view "ExternalLoginConfirmation", (which the user
    /// will also probably want to modify) where they can request additional required information for a 
    /// <see cref="IUserProfile"/> before <see cref="AccountsControllerBase{TUserProfile,TExternalLoginRegistrationModel}"/>
    /// saves the new <see cref="IUserProfile"/>.
    /// </summary>
    public interface IExternalLoginRegistrationModel
    {
        string ExternalLoginData { get; set; }
    }
}