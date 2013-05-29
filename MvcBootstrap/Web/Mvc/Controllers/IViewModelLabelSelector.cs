namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;

    public interface IViewModelLabelSelectorOwner<in TViewModel>
    {
        Func<TViewModel, string> ViewModelLabelSelector { get; }
    }
}