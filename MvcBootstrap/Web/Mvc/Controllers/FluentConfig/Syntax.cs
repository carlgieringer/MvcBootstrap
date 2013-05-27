namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IInitial<TEntity> : IFluentAdapter
    {
        IGivenOptions HasOptions(Func<TEntity, IEnumerable<IEntity>> relatedEntitiesExpression);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IGivenOptions : IUsesLabel, ICanChooseSelf
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IUsesLabel : IFluentAdapter
    {
        IGivenLabel UsesLabel<TRelatedViewModel>(Expression<Func<TRelatedViewModel, string>> labelSelector) where TRelatedViewModel : IEntityViewModel;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IGivenLabel : ICanChooseSelf
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICanChooseSelf : IFluentAdapter
    {
        void CanChooseSelf(bool canChooseSelf);
    }
}