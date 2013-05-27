namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;

    public class RelatedEntitiesConfigFluentAdapter<TEntity> : 
        IInitial<TEntity>,
        IGivenOptions,
        IGivenLabel
    {
        private readonly RelationsConfig<TEntity> config;

        private readonly string memberName;

        public RelatedEntitiesConfigFluentAdapter(RelationsConfig<TEntity> config, string memberName)
        {
            this.config = config;
            this.memberName = memberName;
        }

        private RelationConfig<TEntity> GetOrCreateRelationConfig()
        {
            RelationConfig<TEntity> relationConfig;
            if (!this.config.RelatedConfigsByMemberName.TryGetValue(this.memberName, out relationConfig))
            {
                relationConfig = new RelationConfig<TEntity>();
                this.config.RelatedConfigsByMemberName[this.memberName] = relationConfig;
            }
            return relationConfig;
        }

        public IGivenOptions HasOptions(Func<TEntity, IEnumerable<IEntity>> relatedEntityOptionsSelector)
        {
            var relationConfig = this.GetOrCreateRelationConfig();
            relationConfig.OptionsSelector = relatedEntityOptionsSelector;
            return this;
        }

        public IGivenLabel UsesLabel<TRelatedViewModel>(Expression<Func<TRelatedViewModel, string>> relatedEntityLabelSelector)
            where TRelatedViewModel : IEntityViewModel
        {
            var relationConfig = this.GetOrCreateRelationConfig();
            relationConfig.LabelSelector = relatedEntityLabelSelector;
            return this;
        }

        public void CanChooseSelf(bool canChooseSelf)
        {
            var relationConfig = this.GetOrCreateRelationConfig();
            relationConfig.CanChooseSelf = canChooseSelf;
        }
    }
}