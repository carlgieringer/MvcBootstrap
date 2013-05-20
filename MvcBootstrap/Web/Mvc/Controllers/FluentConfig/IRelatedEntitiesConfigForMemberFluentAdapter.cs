namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using MvcBootstrap.Models;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IRelatedEntitiesConfigForMemberFluentAdapter<TEntity> : IFluentAdapter
    {
        IRelatedEntitiesConfigWithSourceFluentAdapter HasChoices(Func<TEntity, IEnumerable<IEntity>> relatedEntitiesExpression);
    }
}