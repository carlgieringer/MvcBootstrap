namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Collections.Generic;

    using MvcBootstrap.Models;

    public class RelatedEntitiesConfigForMemberFluenAdapter<TEntity, TViewModel> : IRelatedEntitiesConfigForMemberFluentAdapter<TEntity>
    {
        private readonly BootstrapControllerConfig<TEntity, TViewModel> config;

        private readonly string memberName;

        public RelatedEntitiesConfigForMemberFluenAdapter(BootstrapControllerConfig<TEntity, TViewModel> config, string memberName)
        {
            this.config = config;
            this.memberName = memberName;
        }

        public IRelatedEntitiesConfigWithSourceFluentAdapter UseSource(Func<TEntity, IEnumerable<IEntity>> relatedEntitiesSource)
        {
            this.config.RelatedEntitiesSourceSelectorByMemberName[this.memberName] = relatedEntitiesSource;
            return new RelatedEntitiesConfigWithSourceFluentAdapter<TEntity, TViewModel>(this.config, this.memberName);
        }
    }
}