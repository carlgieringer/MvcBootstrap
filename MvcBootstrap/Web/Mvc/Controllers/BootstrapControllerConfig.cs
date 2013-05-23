namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers.FluentConfig;

    public class BootstrapControllerConfig<TEntity, TViewModel> : 
        IViewModelLabelSelector<TViewModel>, 
        IRelatedViewModelLabelSelector
    {
        public BootstrapControllerConfig()
        {
            this.RelatedEntitiesFluentInterface = new RelatedEntitiesConfigFluentAdapter<TEntity, TViewModel>(this);
            this.RelatedEntityOptionsSelectorByMemberName = new Dictionary<string, Func<TEntity, IEnumerable<IEntity>>>();
            this.RelatedEntityViewModelLabelSelectorByMemberName = new Dictionary<string, LambdaExpression>();
        }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.List"/> action renders.
        /// </summary>
        public string ListViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.Create"/> action renders.
        /// </summary>
        public string CreateViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.Read"/> action renders.
        /// </summary>
        public string ReadViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.Update"/> action renders.
        /// </summary>
        public string UpdateViewName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the sort order of this controller's displayed entities.
        /// </summary>
        public SortByClass<TEntity> Sort { get; set; }

        /// <summary>
        /// Gets or sets a lambda that, given an entity, returns a string representation for the entity.
        /// </summary>
        /// <remarks>
        /// <see cref="BootstrapControllerBase{TEntity, TViewModel}"/> uses this to construct messages about modifications to entities.
        /// Because the display of an entity always occurs through its view model, this property may be removed in favor of using only
        /// <see cref="ViewModelLabelSelector"/>.
        /// </remarks>
        public Func<TEntity, string> EntityLabelSelector { get; set; }

        /// <summary>
        /// Gets or sets a lambda that, given an view model, returns a string representation for the view model.
        /// </summary>
        public Func<TViewModel, string> ViewModelLabelSelector { get; set; }

        /// <summary>
        /// Gets a mapping from a string (naming a navigation property) to a lambda that, given an entity, returns an enumerable
        /// of all the related entities which are possible members of the navigation relation.
        /// </summary>
        internal IDictionary<string, Func<TEntity, IEnumerable<IEntity>>> RelatedEntityOptionsSelectorByMemberName { get; private set; }

        /// <summary>
        /// Gets a mapping from a string (naming a navigation property) to a lambda that, given a view model for a related entity,
        /// returns a string representation of the view model.
        /// </summary>
        internal IDictionary<string, LambdaExpression> RelatedEntityViewModelLabelSelectorByMemberName { get; private set; }

        internal RelatedEntitiesConfigFluentAdapter<TEntity, TViewModel> RelatedEntitiesFluentInterface { get; set; }

        /// <summary>
        /// Returns an object exposing a fluent interface for configuring 
        /// </summary>
        /// <returns></returns>
        public IRelatedEntitiesConfigFluentAdapter<TEntity> RelatedEntities()
        {
            return this.RelatedEntitiesFluentInterface;
        }

        public string GetRelatedViewModelLabel<TRelatedViewModel>(string relationMemberName, TRelatedViewModel viewModel)
            where TRelatedViewModel : IEntityViewModel
        {
            var selectorExpression = this.RelatedEntityViewModelLabelSelectorByMemberName[relationMemberName];
            return (string)selectorExpression.Compile().DynamicInvoke(new [] { (object)viewModel });
        }
    }
}