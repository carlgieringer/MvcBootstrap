
namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using MvcBootstrap.ViewModels;

    // TODO remove
    public class RelatedEntityViewModelLabelSelector<TViewModel> where TViewModel : IEntityViewModel
    {
        public readonly Func<TViewModel, string> Selector;

        public RelatedEntityViewModelLabelSelector(Func<TViewModel, string> selector)
        {
            this.Selector = selector;
        }
    }
}
