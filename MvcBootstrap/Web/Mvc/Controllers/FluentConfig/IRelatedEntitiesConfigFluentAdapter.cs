namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IRelatedEntitiesConfigFluentAdapter<TEntity> : IFluentAdapter
    {
        IRelatedEntitiesConfigForMemberFluentAdapter<TEntity> Relation<TRelated>(Expression<Func<TEntity, TRelated>> memberExpression);
    }
        
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IFluentAdapter
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
}