namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;

    using AutoMapper;

    using MvcBootstrap.Data;
    using MvcBootstrap.Extensions;
    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;

    public class MappingCreator<TEntity>
        where TEntity : class, IEntity
    {
        private RelationsConfig<TEntity> relationsConfig;

        public MappingCreator(RelationsConfig<TEntity> relationsConfig)
        {
            this.relationsConfig = relationsConfig;
        }

        public
            IMappingExpression<TAnyEntity, TAnyViewModel>
            CreateEntityToViewModelMap<TAnyEntity, TAnyViewModel>()
            where TAnyEntity : IEntity
            where TAnyViewModel : IEntityViewModel
        {

            return Mapper.CreateMap<TAnyEntity, TAnyViewModel>()
                .ForMember(vm => vm.ConcurrentlyEdited, o => o.Ignore())
                .ForMember(vm => vm.Id, o => o.ResolveUsing(e => e.Id == 0 ? (int?)null : e.Id));
        }

        public
            IMappingExpression<TAnyViewModel, TAnyEntity>
            CreateViewModelToEntityMap<TAnyViewModel, TAnyEntity>()
            where TAnyEntity : IEntity
            where TAnyViewModel : IEntityViewModel
        {
            return Mapper.CreateMap<TAnyViewModel, TAnyEntity>()
                .ForMember(e => e.Created, o => o.Ignore())
                .ForMember(e => e.Modified, o => o.Ignore());
        }

        public
            IMappingExpression<TRelatedEntity, Choice<TRelatedViewModel>>
            CreateEntityToChoiceMap<TRelatedEntity, TRelatedViewModel>()
            where TRelatedEntity : class, IEntity
            where TRelatedViewModel : class, IEntityViewModel, new() 
        {
            var mappingExpression = Mapper.CreateMap<TRelatedEntity, Choice<TRelatedViewModel>>();
            var converter = new EntityToChoiceConverter<TRelatedEntity, TRelatedViewModel>(this);
            mappingExpression.ConvertUsing(converter);
            return mappingExpression;
        }

        public
            IMappingExpression<Choice<TRelatedViewModel>, TRelatedEntity>
            CreateChoiceToEntityMap<TRelatedViewModel, TRelatedEntity>(IBootstrapRepository<TRelatedEntity> repository)
            where TRelatedViewModel : class, IEntityViewModel
            where TRelatedEntity : class, IEntity, new()
        {
            var mappingExpression = Mapper.CreateMap<Choice<TRelatedViewModel>, TRelatedEntity>();
            var converter =
                new ChoiceToEntityConverter<TRelatedViewModel, TRelatedEntity>(repository);
            mappingExpression.ConvertUsing(converter);
            return mappingExpression;
        }

        /// <summary>
        /// Creates a mapping from a collection of entities to a collection of view models where the view model collection requires
        /// the mapping to setup information supporting editing membership of entities in the collection.
        /// </summary>
        /// <param name="roleRepository"></param>
        /// <typeparam name="TRelatedEntity">The type of the related entity</typeparam>
        /// <typeparam name="TRelatedViewModel">The type of the related entity's view model</typeparam>
        /// <returns>
        /// A mapping configuration
        /// </returns>
        public
            IMappingExpression<ICollection<TRelatedEntity>, Choices<TRelatedViewModel>>
            CreateEntitiesToChoicesMap<TRelatedEntity, TRelatedViewModel>()
            where TRelatedEntity : IEntity
            where TRelatedViewModel : class, IEntityViewModel, new()
        {
            var mappingExpression = Mapper.CreateMap<ICollection<TRelatedEntity>, Choices<TRelatedViewModel>>();
            var converter = new EntitiesToChoicesConverter<TRelatedEntity, TRelatedViewModel>(this);
            mappingExpression.ConvertUsing(converter);
            return mappingExpression;
        }

        /// <summary>
        /// Creates a mapping from a collection of view models to a collection of entity stubs; 
        /// </summary>
        /// <remarks>
        /// An entity stub is an instance of an entity with only its <see cref="IEntity.Id"/> set.  It is useful for manipulating relationships
        /// without retrieving entire entities from the database, because you can set it to a navigation property or add it to a navigation
        /// collection and the database will create the relation based upon the Id.  Note that in order to modify the other properties of
        /// the stub entity, it must be attached to the context in the unchanged state (the default of <see cref="DbSet{TEntity}.Attach"/>.
        /// </remarks>
        /// <typeparam name="TRelatedViewModel">The type of the related entity's view model</typeparam>
        /// <typeparam name="TRelatedEntity">The type of the related entity</typeparam>
        /// <returns>
        /// A mapping configuration
        /// </returns>
        public
            IMappingExpression<Choices<TRelatedViewModel>, ICollection<TRelatedEntity>>
            CreateChoicesToEntitiesMap<TRelatedViewModel, TRelatedEntity>(IBootstrapRepository<TRelatedEntity> repository)
            where TRelatedViewModel : IEntityViewModel
            where TRelatedEntity : class, IEntity, new()
        {
            var mappingExpression = Mapper.CreateMap<Choices<TRelatedViewModel>, ICollection<TRelatedEntity>>();
            var converter =
                new ChoicesToEntitiesConverter<TRelatedViewModel, TRelatedEntity>(repository);
            mappingExpression.ConvertUsing(converter);
            return mappingExpression;
        }

        private class 
            EntityToChoiceConverter<TRelatedEntity, TRelatedViewModel> :
            ITypeConverter<TRelatedEntity, Choice<TRelatedViewModel>>
            where TRelatedViewModel : class, IEntityViewModel, new() 
            where TRelatedEntity : class, IEntity
        {
            private readonly MappingCreator<TEntity> mappingCreator;

            public EntityToChoiceConverter(MappingCreator<TEntity> mappingCreator)
            {
                this.mappingCreator = mappingCreator;
            }

            public Choice<TRelatedViewModel> Convert(ResolutionContext context)
            {
                var entity = context.SourceValue as TRelatedEntity;

                var choice = new Choice<TRelatedViewModel>
                    {
                        Selection = Mapper.Map<TRelatedViewModel>(entity),
                        Options = this.mappingCreator.GetChoiceOptions<TRelatedEntity, TRelatedViewModel>(context)
                    };

                return choice;
            }
        }

        private class 
            ChoiceToEntityConverter<TRelatedViewModel, TRelatedEntity> :
            ITypeConverter<Choice<TRelatedViewModel>, TRelatedEntity>
            where TRelatedViewModel : class, IEntityViewModel
            where TRelatedEntity : class, IEntity, new()
        {
            private readonly IBootstrapRepository<TRelatedEntity> repository;

            public ChoiceToEntityConverter(IBootstrapRepository<TRelatedEntity> repository)
            {
                this.repository = repository;
            }

            public TRelatedEntity Convert(ResolutionContext context)
            {
                var entity = context.DestinationValue as TRelatedEntity;

                var choice = context.SourceValue as Choice<TRelatedViewModel> ??
                    new Choice<TRelatedViewModel>();

                var selectedEntity = Mapper.Map<TRelatedEntity>(choice.Selection);
                if (entity != null && selectedEntity == null)
                {
                    entity = null;
                }
                else if ((entity == null && selectedEntity != null) ||
                    (entity != null && entity.Id != selectedEntity.Id))
                {
                    var attachedEntity = this.repository.GetFromLocal(selectedEntity);
                    entity = attachedEntity ??
                        // .Attach puts the DbEntityEntry in the Unchanged state, so properties on entity will not be persisted due to the Attach
                        this.repository.Attach(selectedEntity);
                }

                return entity;
            }
        }

        /// <summary>
        /// Converts a collection of entities to a collection of entity view models where the view model collection contains additional
        /// information supporting editing membership in the collection.
        /// </summary>
        /// <remarks>
        /// Specifically, converts a <see cref="ICollection{TRelatedEnity}"/> to a <see cref="Choices{TRelatedViewModel}"/>. 
        /// This view model collection includes <see cref="Choices{TViewModel}.Options"/> which provides the view models
        /// of entities eligible to participate in the relation.  UI elements should display the <see cref="Choices{TRelatedViewModel}"/>
        /// so as to allow multi-select (e.g., in an HTML tag like <![CDATA[<select multiple>...</select>]]>.
        /// </remarks>
        /// <typeparam name="TRelatedEntity">The type of the related entity</typeparam>
        /// <typeparam name="TRelatedViewModel">The type of the related entity's view model</typeparam>
        private class
            EntitiesToChoicesConverter<TRelatedEntity, TRelatedViewModel> :
            ITypeConverter<ICollection<TRelatedEntity>, Choices<TRelatedViewModel>>
            where TRelatedViewModel : class, IEntityViewModel, new()
        {
            private readonly MappingCreator<TEntity> mappingCreator;

            public EntitiesToChoicesConverter(MappingCreator<TEntity> mappingCreator)
            {
                this.mappingCreator = mappingCreator;
            }

            public Choices<TRelatedViewModel> Convert(ResolutionContext context)
            {
                var choices = new Choices<TRelatedViewModel>();

                var entities = context.SourceValue as ICollection<TRelatedEntity>;
                choices.Selections = Mapper.Map<IEnumerable<TRelatedViewModel>>(entities);

                choices.Options = this.mappingCreator.GetChoiceOptions<TRelatedEntity, TRelatedViewModel>(context);

                return choices;
            }
        }

        private class 
            ChoicesToEntitiesConverter<TRelatedViewModel, TRelatedEntity> :
            ITypeConverter<Choices<TRelatedViewModel>, ICollection<TRelatedEntity>>
            where TRelatedViewModel : IEntityViewModel
            where TRelatedEntity : class, IEntity, new()
        {
            private readonly IBootstrapRepository<TRelatedEntity> repository;

            public ChoicesToEntitiesConverter(IBootstrapRepository<TRelatedEntity> repository)
            {
                this.repository = repository;
            }

            public ICollection<TRelatedEntity> Convert(ResolutionContext context)
            {
                var entities = context.DestinationValue as ICollection<TRelatedEntity> ??
                    new Collection<TRelatedEntity>();

                // Start with an empty collection so that the only ones in it at the end are those that were mapped
                entities.Clear();

                var viewModelCollection = context.SourceValue as Choices<TRelatedViewModel> ??
                    new Choices<TRelatedViewModel>();

                foreach (var viewModel in viewModelCollection.Selections)
                {
                    var entity = Mapper.Map<TRelatedEntity>(viewModel);
                    if (entities.Any(e => e.Id == entity.Id))
                    {
                        continue;
                    }

                    var attachedEntity = this.repository.GetFromLocal(entity);
                    entity = attachedEntity ??
                        // .Attach puts the DbEntityEntry in the Unchanged state, so properties on entity will not be persisted due to the Attach
                        this.repository.Attach(entity);

                    // Adding the entity to a collection that is a navigation property will be picked up by the context
                    entities.Add(entity);
                }

                return entities;
            }
        }

        private IEnumerable<TRelatedViewModel>
            GetChoiceOptions<TRelatedEntity, TRelatedViewModel>(ResolutionContext context) 
            where TRelatedViewModel : class, IEntityViewModel, new()
        {
            IEnumerable<IEntity> entityOptions;

            RelationConfig<TEntity> relationConfig;
            if (this.relationsConfig.RelatedConfigsByMemberName.TryGetValue(
                context.MemberName,
                out relationConfig))
            {
                var entity = context.Parent.SourceValue as TEntity;
                entityOptions = relationConfig.OptionsSelector(entity);

                if (typeof(TRelatedEntity).IsAssignableTo(typeof(TEntity)) &&
                    relationConfig.CanChooseSelf.HasValue &&
                    !relationConfig.CanChooseSelf.Value &&
                    entity != null)
                {
                    entityOptions = entityOptions.Where(e => e.Id != entity.Id);
                }
            }
            else
            {
                entityOptions = Enumerable.Empty<IEntity>();
            }

            var viewModelOptions = Mapper.Map<IEnumerable<TRelatedViewModel>>(entityOptions);
            return viewModelOptions;
        }
    }
}
