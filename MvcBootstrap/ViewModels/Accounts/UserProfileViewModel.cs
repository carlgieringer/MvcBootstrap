namespace MvcBootstrap.ViewModels.Accounts
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UserProfileViewModel : EntityViewModelBase, IUserProfileViewModel
    {
        [Required, MaxLength(128)]
        public string Username { get; set; }

        [DataType(DataType.EmailAddress), Required]
        public string Email { get; set; }

        public DateTime? LastLogin { get; set; }

    }
}
