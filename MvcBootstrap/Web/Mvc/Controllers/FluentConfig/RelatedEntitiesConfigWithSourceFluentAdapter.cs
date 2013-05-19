namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;

    public class RelatedEntitiesConfigWithSourceFluentAdapter<TEntity, TViewModel> : IRelatedEntitiesConfigWithSourceFluentAdapter
    {
        private readonly BootstrapControllerConfig<TEntity, TViewModel> config;

        private readonly string memberName;

        public RelatedEntitiesConfigWithSourceFluentAdapter(BootstrapControllerConfig<TEntity, TViewModel> config, string memberName)
        {
            this.config = config;
            this.memberName = memberName;
        }

        public void WithLabel<TRelatedViewModel>(Expression<Func<TRelatedViewModel, string>> labelSelector) 
            where TRelatedViewModel : IEntityViewModel
        {
            this.config.RelatedEntityViewModelLabelSelectorByMemberName[this.memberName] = labelSelector;
        }
    }
}