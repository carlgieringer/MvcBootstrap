namespace MvcBootstrap.Controllers
{
    using System;

    public interface IViewModelLabelSelector<in TViewModel>
    {
        Func<TViewModel, string> ViewModelLabelSelector { get; }
    }
}