namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Linq.Expressions;

    using MvcBootstrap.ViewModels;

    public interface IRelatedEntitiesConfigWithSourceFluentAdapter
    {
        void WithLabel<TRelatedViewModel>(Expression<Func<TRelatedViewModel, string>> labelSelector) where TRelatedViewModel : IEntityViewModel;
    }
}