namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using System.Web.Security;

    using AutoMapper;

    using DotNetOpenAuth.AspNet;

    using Microsoft.Web.WebPages.OAuth;

    using MvcBootstrap.Data;
    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.ViewModels.Accounts;

    using TEMTDomain.StaticLib;

    using WebMatrix.WebData;

    /// <summary>
    /// A controller implementation that handles most of the details of windows SimpleMembershipProvider,
    /// managing user accounts and third-party authentications.
    /// </summary>
    /// <typeparam name="TUserProfile"></typeparam>
    /// <typeparam name="TExternalLoginRegistrationModel">
    /// The type instantiated and passed to the view "ExternalLoginConfirmation",
    /// and upon posting to that action, mapped to the new <see cref="TUserProfile"/>
    /// before it is saved to the database.  This process provides subclassers the 
    /// opportunity to ellicit additional information from users when they write the
    /// view "ExternalLoginConfirmation" taking their implementation of <see cref="TExternalLoginRegistrationModel"/>.
    /// </typeparam>
    [Authorize]
    public abstract class AccountsControllerBase<TUserProfile, TUserProfileViewModel> : Controller
        where TUserProfile : class, IUserProfile
        where TUserProfileViewModel : class, IUserProfileViewModel, new()
    {
        static AccountsControllerBase()
        {
            Mapper.CreateMap<TUserProfileViewModel, TUserProfile>();
        }


        #region Constructors

        public AccountsControllerBase(IUserProfileRepository<TUserProfile> repository)
        {
            this.Repository = repository;
            this.Config = new AccountsConfiguration();
        }

        #endregion


        #region Properties

        protected IUserProfileRepository<TUserProfile> Repository { get; private set; }

        public AccountsConfiguration Config { get; private set; }

        #endregion


        #region Actions

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (this.ModelState.IsValid
                && WebSecurity.Login(model.Username, model.Password, persistCookie: model.RememberMe))
            {
                return this.PostSuccessfulLogin(model.Username, returnUrl);
            }

            // If we got this far, something failed, redisplay form
            this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            WebSecurity.Logout();

            return this.RedirectToAction("Index", "Home");
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Register()
        {
            return this.View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Register(RegistrationModel<TUserProfileViewModel> registrationModel)
        {
            if (this.ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    var profile = this.CreateUserProfile(registrationModel.UserProfileViewModel);

                    WebSecurity.CreateAccount(profile.Username, registrationModel.Password, this.Config.RequireAccountConfirmation);
                    WebSecurity.Login(profile.Username, registrationModel.Password);

                    return this.PostSuccessfulLogin(profile.Username, null);
                }
                catch (MembershipCreateUserException ex)
                {
                    this.ModelState.AddModelError(string.Empty, ErrorCodeToString(ex.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return this.View(registrationModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == this.User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (
                    var scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount =
                        OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(this.User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return this.RedirectToAction("Manage", new { Message = message });
        }

        [HttpGet]
        public ActionResult Manage(ManageMessageId? message)
        {
            this.ViewBag.StatusMessage = message == ManageMessageId.ChangePasswordSuccess
                                             ? "Your password has been changed."
                                             : message == ManageMessageId.SetPasswordSuccess
                                                   ? "Your password has been set."
                                                   : message == ManageMessageId.RemoveLoginSuccess
                                                         ? "The external login was removed."
                                                         : string.Empty;
            this.ViewBag.HasLocalPassword =
                OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
            this.ViewBag.ReturnUrl = this.Url.Action("Manage");
            return this.View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
            this.ViewBag.HasLocalPassword = hasLocalAccount;
            this.ViewBag.ReturnUrl = this.Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (this.ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(
                            this.User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return this.RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        this.ModelState.AddModelError(
                            string.Empty, "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = this.ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (this.ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(this.User.Identity.Name, model.NewPassword);
                        return this.RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        this.ModelState.AddModelError(
                            string.Empty,
                            string.Format(
                                "Unable to create local account. An account with the name \"{0}\" may already exist.",
                                this.User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(
                provider, this.Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        [HttpGet, AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            string callbackUrl = this.Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl });
            var result = OAuthWebSecurity.VerifyAuthentication(callbackUrl);
            if (!result.IsSuccessful)
            {
                return this.RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, this.Config.CreatePersistentCookie))
            {
                string username = OAuthWebSecurity.GetUserName(result.Provider, result.ProviderUserId);
                return this.PostSuccessfulLogin(username, returnUrl);
            }
            
            if (this.User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, this.User.Identity.Name);
                string username = OAuthWebSecurity.GetUserName(result.Provider, result.ProviderUserId);
                return this.PostSuccessfulLogin(username, returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                this.ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                this.ViewBag.ReturnUrl = returnUrl;

                var registrationModel = new ExternalLoginRegistrationModel<TUserProfileViewModel>
                    {
                        UserProfileViewModel = new TUserProfileViewModel(),
                        ExternalLoginData = loginData
                    };
                registrationModel.UserProfileViewModel.Username = result.UserName;

                return this.View("ExternalLoginConfirmation", registrationModel);
            }
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(ExternalLoginRegistrationModel<TUserProfileViewModel> registrationModel, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (this.User.Identity.IsAuthenticated || 
                !OAuthWebSecurity.TryDeserializeProviderUserId(registrationModel.ExternalLoginData, out provider, out providerUserId))
            {
                return this.RedirectToAction("Manage");
            }

            if (this.ModelState.IsValid)
            {
                //// Insert a new user into the database

                var profile = this.Repository.GetByUsername(registrationModel.UserProfileViewModel.Username);

                // Check if user already exists
                if (profile == null)
                {
                    profile = this.CreateUserProfile(registrationModel.UserProfileViewModel);

                    OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, profile.Username);
                    OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                    return this.PostSuccessfulLogin(profile.Username, returnUrl);
                }
                else
                {
                    this.ModelState.AddModelError(
                        Of<TUserProfileViewModel>.CodeNameFor(m => m.Username),
                        "User name already exists. Please enter a different user name.");
                }
            }

            this.ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View(registrationModel);
        }

        [HttpGet, AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return this.View();
        }

        [AllowAnonymous, ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            var accounts = OAuthWebSecurity.GetAccountsFromUserName(this.User.Identity.Name);
            var externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(
                    new ExternalLogin
                        {
                            Provider = account.Provider,
                            ProviderDisplayName = clientData.DisplayName,
                            ProviderUserId = account.ProviderUserId,
                        });
            }

            this.ViewBag.ShowRemoveButton = 
                externalLogins.Count > 1 || 
                OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(this.User.Identity.Name));
            return this.PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #endregion


        #region Helpers

        private TUserProfile CreateUserProfile(TUserProfileViewModel userProfileViewModel)
        {
            // Insert name into the profile table
            var profile = this.Repository.CreateAndAdd();
            // Transfer the values from TExternalLoginRegistration to the new profile
            Mapper.Map(userProfileViewModel, profile);
            this.Repository.SaveChanges();
            return profile;
        }

        private ActionResult PostSuccessfulLogin(string username, string returnUrl)
        {
            this.UpdateLastLogin(username);

            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        private void UpdateLastLogin(string username)
        {
            var userProfile = this.Repository.GetByUsername(username);
            userProfile.LastLogin = DateTime.Now;
            this.Repository.SaveChanges();
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,

            SetPasswordSuccess,
            
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                this.Provider = provider;
                this.ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }

            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(this.Provider, this.ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion
    }
}
