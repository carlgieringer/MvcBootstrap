namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Collections.Generic;

    using MvcBootstrap.Models;

    public interface IRelatedEntitiesConfigForMemberFluentAdapter<TEntity>
    {
        IRelatedEntitiesConfigWithSourceFluentAdapter UseSource(Func<TEntity, IEnumerable<IEntity>> relatedEntitiesExpression);
    }
}