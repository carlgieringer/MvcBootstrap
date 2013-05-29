namespace MvcBootstrap.Web.Mvc.Controllers
{
    using MvcBootstrap.ViewModels;

    public interface IViewModelLabelSelectorContainer<in TViewModel>
        where TViewModel : class, IEntityViewModel
    {
        IViewModelLabelSelectorOwner<TViewModel> ViewModelLabelSelector { get; }
    }
}
