namespace MvcBootstrap.Controllers
{
    using MvcBootstrap.ViewModels;

    public interface IViewModelLabelSelectorContainer<in TViewModel>
        where TViewModel : class, IEntityViewModel
    {
        IViewModelLabelSelector<TViewModel> LabelSelector { get; }
    }
}
