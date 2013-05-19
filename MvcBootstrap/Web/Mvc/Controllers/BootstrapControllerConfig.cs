namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers.FluentConfig;

    public class BootstrapControllerConfig<TEntity, TViewModel> : IViewModelLabelSelector<TViewModel>, IRelatedViewModelLabelSelector
    {
        public BootstrapControllerConfig()
        {
            this.RelatedEntitiesFluentInterface = new RelatedEntitiesConfigFluentAdapter<TEntity, TViewModel>(this);
            this.RelatedEntitiesSourceSelectorByMemberName = new Dictionary<string, Func<TEntity, IEnumerable<IEntity>>>();
            this.RelatedEntityViewModelLabelSelectorByMemberName = new Dictionary<string, LambdaExpression>();
        }

        public string ListViewName { get; set; }

        public string CreateViewName { get; set; }

        public string ReadViewName { get; set; }

        public string UpdateViewName { get; set; }

        public SortByClass<TEntity> Sort { get; set; }

        public Func<TEntity, string> EntityLabelSelector { get; set; }

        public Func<TViewModel, string> ViewModelLabelSelector { get; set; }

        public IDictionary<Type, Func<object, string>> RelatedViewModelLabelSelectorByType { get; set; }

        internal Dictionary<string, Func<TEntity, IEnumerable<IEntity>>> RelatedEntitiesSourceSelectorByMemberName { get; set; }

        internal Dictionary<string, LambdaExpression> RelatedEntityViewModelLabelSelectorByMemberName { get; set; }

        internal RelatedEntitiesConfigFluentAdapter<TEntity, TViewModel> RelatedEntitiesFluentInterface { get; set; }

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