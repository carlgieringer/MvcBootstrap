namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Models;

    public class RelatedEntitiesConfigFluentAdapter<TEntity, TViewModel> : IRelatedEntitiesConfigFluentAdapter<TEntity>
    {
        private readonly BootstrapControllerConfig<TEntity, TViewModel> config;

        public RelatedEntitiesConfigFluentAdapter(BootstrapControllerConfig<TEntity, TViewModel> config)
        {
            this.config = config;
        }

        public IRelatedEntitiesConfigForMemberFluentAdapter<TEntity> Relation<TRelated>(Expression<Func<TEntity, TRelated>> memberExpression)
        {
            Type relatedType = typeof(TRelated);

            if (relatedType.IsAssignableTo(typeof(IEntity)))
            {
                throw new NotImplementedException("TODO");
            }
            else if (relatedType.IsConstructedGenericTypeFor(typeof(ICollection<>), typeof(IEntity)))
            {
                var memberName = ExpressionHelper.GetExpressionText(memberExpression);
                return new RelatedEntitiesConfigForMemberFluenAdapter<TEntity, TViewModel>(this.config, memberName);
            }
            else
            {
                throw new InvalidOperationException("Only related entities or ICollections of entities is supported.");
            }
        }
    }
}