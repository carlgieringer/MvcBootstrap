namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Linq.Expressions;

    using MvcBootstrap.Web.Mvc.Controllers.FluentConfig;

    public class BootstrapControllerConfig<TEntity, TViewModel> : 
        IViewModelLabelSelectorOwner<TViewModel>
    {
        public BootstrapControllerConfig()
        {
            this.RelationsConfig = new RelationsConfig<TEntity>();
        }

        public RelationsConfig<TEntity> RelationsConfig { get; private set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.List"/> action renders.
        /// </summary>
        public string ListViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.Create()"/> action renders.
        /// </summary>
        public string CreateViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.Read"/> action renders.
        /// </summary>
        public string ReadViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.Update(Int32)"/> action renders.
        /// </summary>
        public string UpdateViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view that the <see cref="BootstrapControllerBase{TEntity, TViewModel}.Delete(Int32)"/> action renders.
        /// </summary>
        public string DeleteViewName { get; set; }

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
        /// Returns the result of calling <see cref="RelationsConfig"/>'s <see cref="RelationsConfig{TEntity}.Relation{TRelated}"/>
        /// with <paramref name="memberExpression"/>.
        /// </summary>
        /// <returns></returns>
        public IInitial<TEntity> Relation<TRelated>(Expression<Func<TEntity, TRelated>> memberExpression)
        {
            return this.RelationsConfig.Relation(memberExpression);
        }
    }
}