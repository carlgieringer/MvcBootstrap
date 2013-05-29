namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers.FluentConfig;

    public class RelationsConfig<TEntity> :
        IRelatedViewModelLabelSelector
    {
        public RelationsConfig()
        {
            this.RelatedConfigsByMemberName = new Dictionary<string, RelationConfig<TEntity>>(); 
        }

        internal IDictionary<string, RelationConfig<TEntity>> RelatedConfigsByMemberName { get; private set; }

        public string GetRelatedViewModelLabel<TRelatedViewModel>(string relationMemberName, TRelatedViewModel viewModel)
            where TRelatedViewModel : IEntityViewModel
        {
            var selectorExpression = this.RelatedConfigsByMemberName[relationMemberName].LabelSelector;
            return (string)selectorExpression.Compile().DynamicInvoke(new[] { (object)viewModel });
        }

        /// <summary>
        /// Returns an object exposing a fluent interface for configuring the mapping of related entities.
        /// </summary>
        /// <returns></returns>
        public IInitial<TEntity> Relation<TRelated>(Expression<Func<TEntity, TRelated>> memberExpression)
        {
            Type relatedType = typeof(TRelated);

            if (relatedType.IsAssignableTo(typeof(IEntity)))
            {
                var memberName = ExpressionHelper.GetExpressionText(memberExpression);
                return new RelatedEntitiesConfigFluentAdapter<TEntity>(this, memberName);
            }
            else if (relatedType.IsConstructedGenericTypeOfDefinitionWith(typeof(ICollection<>), typeof(IEntity)))
            {
                var memberName = ExpressionHelper.GetExpressionText(memberExpression);
                return new RelatedEntitiesConfigFluentAdapter<TEntity>(this, memberName);
            }
            else
            {
                throw new InvalidOperationException("Only related IEntity or ICollection of IEntity is supported.");
            }
        }
    }
}
