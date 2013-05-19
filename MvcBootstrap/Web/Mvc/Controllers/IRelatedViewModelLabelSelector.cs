namespace MvcBootstrap.Web.Mvc.Controllers
{
    using MvcBootstrap.ViewModels;

    public interface IRelatedViewModelLabelSelector
    {
        string GetRelatedViewModelLabel<TRelatedViewModel>(string propertyName, TRelatedViewModel viewModel) where TRelatedViewModel : IEntityViewModel;
    }
}