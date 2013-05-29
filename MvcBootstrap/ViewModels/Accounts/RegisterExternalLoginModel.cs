namespace MvcBootstrap.ViewModels.Accounts
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterExternalLoginModel
    {
        [Required]
        public string Username { get; set; }

        public string ExternalLoginData { get; set; }
    }
}