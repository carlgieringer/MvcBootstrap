namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    using MvcBootstrap.ViewModels;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IRelatedEntitiesConfigWithSourceFluentAdapter : IFluentAdapter
    {
        void UsesLabel<TRelatedViewModel>(Expression<Func<TRelatedViewModel, string>> labelSelector) where TRelatedViewModel : IEntityViewModel;
    }
}