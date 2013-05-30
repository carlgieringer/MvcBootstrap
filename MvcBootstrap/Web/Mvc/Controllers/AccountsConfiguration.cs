namespace MvcBootstrap.Web.Mvc.Controllers
{
    public class AccountsConfiguration
    {
        /// <summary>
        /// If true, users must receive a confirmation email and visit the link 
        /// therein to confirm their account.
        /// </summary>
        public bool RequireAccountConfirmation { get; set; }

        public bool CreatePersistentCookie { get; set; }
    }
}