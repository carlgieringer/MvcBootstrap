namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System.Web.Mvc;

    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;

    public interface IBootstrapController<TViewModel> : 
        IController, 
        IViewModelLabelSelectorContainer<TViewModel>, 
        IRelatedViewModelLabelSelectorContainer
        where TViewModel : class, IEntityViewModel
    {
    }
}
