namespace MvcBootstrap.Tests.Mapping
{
    using MvcBootstrap.ViewModels;

    public class SelfReferentialEntityViewModel : EntityViewModelBase
    {
        public Choice<SelfReferentialEntityViewModel> Other { get; set; } 
    }
}