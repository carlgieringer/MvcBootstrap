namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Linq.Expressions;

    public interface IRelatedEntitiesConfigFluentAdapter<TEntity>
    {
        IRelatedEntitiesConfigForMemberFluentAdapter<TEntity> For<TRelated>(Expression<Func<TEntity, TRelated>> memberExpression);
    }
}