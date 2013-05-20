namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using AutoMapper;

    using MvcBootstrap.Data;
    using MvcBootstrap.Exceptions;
    using MvcBootstrap.Extensions;
    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;

    public abstract class BootstrapControllerBase<TEntity, TViewModel> : Controller, IBootstrapController<TViewModel>
        where TEntity : class, IEntity, new()
        where TViewModel : class, IEntityViewModel
    {
        #region Fields

        protected readonly IBootstrapRepository<TEntity> Repository;

        protected readonly IMappingExpression<TEntity, TViewModel> EntityToViewModelMappingExpression;

        protected readonly IMappingExpression<TViewModel, TEntity> ViewModelToEntityMappingExpression;

        #endregion


        #region Constructors

        public BootstrapControllerBase(IBootstrapRepository<TEntity> repository)
        {
            this.Repository = repository;

            this.Config = new BootstrapControllerConfig<TEntity, TViewModel>
                {
                    CreateViewName = "Create",
                    ReadViewName = "Read",
                    UpdateViewName = "Update",
                    ListViewName = "List",
                    EntityLabelSelector = e => e.Id.ToString(),
                    ViewModelLabelSelector = vm => vm.Id.ToString(),
                    Sort = Sort.ByDescending(e => e.Created)
                };

            this.EntityToViewModelMappingExpression = this.CreateEntityToViewModelMap<TEntity, TViewModel>();
            this.ViewModelToEntityMappingExpression = this.CreateViewModelToEntityMap<TViewModel, TEntity>();
        }

        #endregion


        #region Properties

        public IViewModelLabelSelector<TViewModel> ViewModelLabelSelector
        {
            get
            {
                return this.Config;
            }
        }

        public IRelatedViewModelLabelSelector RelatedViewModelLabelSelector
        {
            get
            {
                return this.Config;
            }
        }

        public BootstrapControllerConfig<TEntity, TViewModel> Config { get; private set; }

        private BootstrapRepositoryBase<TEntity> RepositoryInternal
        {
            get
            {
                return this.Repository as BootstrapRepositoryBase<TEntity>;
            }
        } 

        #endregion


        #region Actions

        public ActionResult Index()
        {
            return this.RedirectToAction("List");
        }

        [HttpGet]
        public ActionResult List()
        {
            var entities = this.Repository.Items.AsEnumerable();

            if (this.Config.Sort != null)
            {
                entities = this.ApplySort(entities);
            }

            var viewModels = Mapper.Map<TViewModel[]>(entities);
            return this.View(this.Config.ListViewName, viewModels);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var entity = this.Repository.Create();
            var viewModel = Mapper.Map<TViewModel>(entity);
            this.ViewBag.Action = "Create";
            return this.View(this.Config.CreateViewName, viewModel);
        }

        [HttpPost]
        public ActionResult Create(TViewModel viewModel)
        {
            if (viewModel.Id.HasValue)
            {
                return this.HttpNotFound();
            }

            if (this.ModelState.IsValid)
            {
                var entity = Mapper.Map<TEntity>(viewModel);
                entity = this.Repository.Add(entity);
                this.Repository.SaveChanges();

                this.Flash(
                    @"{0} ""{1}"" Created".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)),
                    FlashKind.Success);

                return this.RedirectToAction("List");
            }

            return this.View(this.Config.CreateViewName, viewModel);
        }

        public ActionResult Read(int id)
        {
            var entity = this.Repository.GetById(id);
            if (entity == null)
            {
                return this.HttpNotFound();
            }

            var viewModel = Mapper.Map<TViewModel>(entity);
            return this.View(this.Config.ReadViewName, viewModel);
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var entity = this.Repository.GetById(id);
            if (entity == null)
            {
                return this.HttpNotFound();
            }

            var viewModel = Mapper.Map<TViewModel>(entity);
            this.ViewBag.Action = "Edit";
            return this.View(this.Config.UpdateViewName, viewModel);
        }

        [HttpPost]
        public ActionResult Update(TViewModel viewModel)
        {
            if (!viewModel.Id.HasValue)
            {
                return this.HttpNotFound();
            }

            var entity = this.Repository.GetById(viewModel.Id.Value);

            if (this.ModelState.IsValid)
            {
                entity = Mapper.Map(viewModel, entity);
                entity = this.Repository.Update(entity);

                try
                {
                    this.Repository.SaveChanges();
                }
                catch (ConcurrentUpdateException ex)
                {
                    var originalViewModel = Mapper.Map<TViewModel>(ex.Entity);
                    viewModel.ConcurrentlyEdited = originalViewModel;

                    // Update the timestamp so that the user can submit the newly entered values.
                    viewModel.Timestamp = originalViewModel.Timestamp;

                    const string ErrorMessageFormat = "The {0} has been edited since you requested it.  "
                                             + "Please ensure the correct values are below.  "
                                             + "Cancel to leave the new current values.";
                    this.ModelState.AddModelError(
                        string.Empty, 
                        string.Format(ErrorMessageFormat, typeof(TViewModel).Description()));

                    this.Flash(
                        "An edit conflict occurred.",
                        FlashKind.Error);
                }
                catch (DbEntityValidationException ex)
                {
                    var errorKeyAndMessages = GetEntityValidationErrorKeyAndMessages(ex, entity);
                    foreach (var errorKeyAndMessage in errorKeyAndMessages)
                    {
                        this.ModelState.AddModelError(errorKeyAndMessage.ErrorKey, errorKeyAndMessage.ErrorMessage);
                    }

                    this.Flash(
                        @"Failed to save {0} ""{1}""".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)),
                        FlashKind.Error);
                }

                if (this.ModelState.IsValid)
                {
                    this.Flash(
                        @"{0} ""{1}"" Updated".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)),
                        FlashKind.Success);

                    return this.RedirectToAction("List");
                }
            }

            // For now the easiest way to populate related entity view model choices is to map from an entity
            // So start with a fresh entity...
            var tempEntity = this.Repository.GetById(viewModel.Id.Value);

            // Map the view model changes to it...
            tempEntity = Mapper.Map(viewModel, tempEntity);
            
            // And then map back to the viewModel to set the related entity view model choices.
            viewModel = Mapper.Map(tempEntity, viewModel);

            return this.View(this.Config.UpdateViewName, viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var entity = this.Repository.GetById(id);
            if (entity == null)
            {
                return this.HttpNotFound();
            }

            var entityLabel = this.Config.EntityLabelSelector(entity);

            this.Repository.Delete(entity);
            this.Repository.SaveChanges();

            this.Flash(@"{0} ""{1}"" Deleted".F(typeof(TEntity).Description(), entityLabel), FlashKind.Success);
            return this.RedirectToAction("List");
        }

        #endregion


        #region Mapping Methods

        protected 
            IMappingExpression<TAnyEntity, TAnyViewModel> 
            CreateEntityToViewModelMap<TAnyEntity, TAnyViewModel>()
            where TAnyEntity : IEntity 
            where TAnyViewModel : IEntityViewModel
        {

            return
                Mapper.CreateMap<TAnyEntity, TAnyViewModel>()
                      .ForMember(vm => vm.ConcurrentlyEdited, o => o.Ignore())
                      .ForMember(vm => vm.Id, o => o.ResolveUsing(e => e.Id == 0 ? (int?)null : e.Id));
        }

        protected 
            IMappingExpression<TAnyViewModel, TAnyEntity> 
            CreateViewModelToEntityMap<TAnyViewModel, TAnyEntity>()
            where TAnyEntity : IEntity 
            where TAnyViewModel : IEntityViewModel
        {
            return
                Mapper.CreateMap<TAnyViewModel, TAnyEntity>()
                      .ForMember(e => e.Created, o => o.Ignore())
                      .ForMember(e => e.Modified, o => o.Ignore());
        }

        /// <summary>
        /// Creates a mapping from a collection of entities to a collection of view models where the view model collection requires
        /// the mapping to setup information supporting editing membership of entities in the collection.
        /// </summary>
        /// <typeparam name="TRelatedEntity">The type of the related entity</typeparam>
        /// <typeparam name="TRelatedViewModel">The type of the related entity's view model</typeparam>
        /// <returns>
        /// A mapping configuration
        /// </returns>
        protected 
            IMappingExpression<ICollection<TRelatedEntity>, ChoiceCollection<TRelatedViewModel>>
            CreateRelatedEntityCollectionToChoiceCollectionMap<TRelatedEntity, TRelatedViewModel>()
            where TRelatedEntity : IEntity
            where TRelatedViewModel : IEntityViewModel
        {
            var mappingExpression = Mapper.CreateMap<ICollection<TRelatedEntity>, ChoiceCollection<TRelatedViewModel>>();
            var converter = new EntityCollectionToChoiceCollectionConverter<TRelatedEntity, TRelatedViewModel>(this.Config);
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
        protected 
            IMappingExpression<ChoiceCollection<TRelatedViewModel>, ICollection<TRelatedEntity>>
            CreateChoiceCollectionToEntityCollectionMap<TRelatedViewModel, TRelatedEntity>()
            where TRelatedViewModel : IEntityViewModel
            where TRelatedEntity : class, IEntity, new()
        {
            var mappingExpression = Mapper.CreateMap<ChoiceCollection<TRelatedViewModel>, ICollection<TRelatedEntity>>();
            var converter = 
                new ChoiceCollectionToEntityCollectionConverter<TRelatedViewModel, TRelatedEntity>(this.RepositoryInternal.Context);
            mappingExpression.ConvertUsing(converter);
            return mappingExpression;
        }

        /// <summary>
        /// Converts a collection of entities to a collection of entity view models where the view model collection contains additional
        /// information supporting editing membership in the collection.
        /// </summary>
        /// <remarks>
        /// Specifically, converts a <see cref="ICollection{TRelatedEnity}"/> to a <see cref="ChoiceCollection{TRelatedViewModel}"/>. 
        /// This view model collection includes <see cref="ChoiceCollection{TViewModel}.Choices"/> which provides the view models
        /// of entities eligible to participate in the relation.  UI elements should display the <see cref="ChoiceCollection{TRelatedViewModel}"/>
        /// so as to allow multi-select (e.g., in an HTML tag like <![CDATA[<select multiple>...</select>]]>.
        /// </remarks>
        /// <typeparam name="TRelatedEntity">The type of the related entity</typeparam>
        /// <typeparam name="TRelatedViewModel">The type of the related entity's view model</typeparam>
        private class EntityCollectionToChoiceCollectionConverter<TRelatedEntity, TRelatedViewModel> : 
            ITypeConverter<ICollection<TRelatedEntity>, ChoiceCollection<TRelatedViewModel>>
            where TRelatedViewModel : IEntityViewModel
        {
            private readonly BootstrapControllerConfig<TEntity, TViewModel> controllerConfig;

            public EntityCollectionToChoiceCollectionConverter(BootstrapControllerConfig<TEntity, TViewModel> controllerConfig)
            {
                this.controllerConfig = controllerConfig;
            }

            public ChoiceCollection<TRelatedViewModel> Convert(ResolutionContext context)
            {
                var viewModelCollection = new ChoiceCollection<TRelatedViewModel>();

                var entityCollection = context.SourceValue as ICollection<TRelatedEntity>;
                viewModelCollection = MapEntityCollectionContentsToViewModelCollection(entityCollection, viewModelCollection);

                viewModelCollection.Choices = this.GetViewModelChoices(context);

                return viewModelCollection;
            }

            private static 
                ChoiceCollection<TRelatedViewModel> 
                MapEntityCollectionContentsToViewModelCollection(
                    ICollection<TRelatedEntity> entityCollection, 
                    ChoiceCollection<TRelatedViewModel> viewModelCollection)
            {
                if (entityCollection != null)
                {
                    foreach (var entity in entityCollection)
                    {
                        viewModelCollection.Add(Mapper.Map<TRelatedViewModel>(entity));
                    }
                }

                return viewModelCollection;
            }

            private IEnumerable<IEntityViewModel> GetViewModelChoices(ResolutionContext context)
            {
                IEnumerable<IEntity> entityChoices;

                Func<TEntity, IEnumerable<IEntity>> relatedEntityChoicesSelector;
                if (this.controllerConfig.RelatedEntityChoicesSelectorByMemberName.TryGetValue(
                    context.MemberName, 
                    out relatedEntityChoicesSelector))
                {
                    var entity = context.Parent.SourceValue as TEntity;
                    entityChoices = relatedEntityChoicesSelector(entity);
                }
                else
                {
                    entityChoices = Enumerable.Empty<IEntity>();
                }

                var viewModelChoices = Mapper.Map<IEnumerable<TRelatedViewModel>>(entityChoices);
                return viewModelChoices.Cast<IEntityViewModel>().AsEnumerable();
            }
        }
        
        private class ChoiceCollectionToEntityCollectionConverter<TRelatedViewModel, TRelatedEntity> :
            ITypeConverter<ChoiceCollection<TRelatedViewModel>, ICollection<TRelatedEntity>>
            where TRelatedViewModel : IEntityViewModel
            where TRelatedEntity : class, IEntity, new()
        {
            private readonly DbContext dbContext;

            public ChoiceCollectionToEntityCollectionConverter(DbContext context)
            {
                this.dbContext = context;
            }

            public ICollection<TRelatedEntity> Convert(ResolutionContext context)
            {
                var entityCollection = context.DestinationValue as ICollection<TRelatedEntity>;
                if (entityCollection == null)
                {
                    throw new MvcBootstrapException("Mapping error: entity relation collection was not convertable to ICollection<TRelatedEntity>.");
                }

                // Start with an empty collection so that the only ones in it at the end are those that were mapped
                entityCollection.Clear();

                var viewModelCollection = context.SourceValue as ChoiceCollection<TRelatedViewModel>;
                if (viewModelCollection == null)
                {
                    throw new MvcBootstrapException("View model collection was not convertible to EntityViewModelChoiceCollection");
                }

                foreach (var viewModel in viewModelCollection)
                {
                    var entity = Mapper.Map<TRelatedEntity>(viewModel);
                    if (entityCollection.Any(e => e.Id == entity.Id))
                    {
                        continue;
                    }
                    
                    var attachedEntity = this.dbContext.Set<TRelatedEntity>().Local.SingleOrDefault(e => e.Id == entity.Id);
                    entity = attachedEntity ?? 
                        // .Attach puts the DbEntityEntry in the Unchanged state, so properties on entity will not be persisted due to the Attach
                        this.dbContext.Set<TRelatedEntity>().Attach(entity);

                    // Adding the entity to a collection that is a navigation property will be picked up by the context
                    entityCollection.Add(entity);
                }

                return entityCollection;
            }
        }

        #endregion


        #region Private Methods

        private IEnumerable<TEntity> ApplySort(IEnumerable<TEntity> entities)
        {
            IOrderedEnumerable<TEntity> sortedEntities;
            switch (this.Config.Sort.SortOrder)
            {
                case SortOrder.Ascending:
                    sortedEntities = entities.OrderBy(this.Config.Sort.SortBy);
                    break;
                case SortOrder.Descending:
                    sortedEntities = entities.OrderByDescending(this.Config.Sort.SortBy);
                    break;
                default:
                    throw new UnhandledEnumException<SortOrder>(this.Config.Sort.SortOrder);
            }

            foreach (var sort in this.Config.Sort.ThenBys)
            {
                switch (sort.SortOrder)
                {
                    case SortOrder.Ascending:
                        sortedEntities = sortedEntities.ThenBy(sort.SortBy);
                        break;
                    case SortOrder.Descending:
                        sortedEntities = sortedEntities.OrderByDescending(sort.SortBy);
                        break;
                    default:
                        throw new UnhandledEnumException<SortOrder>(sort.SortOrder);
                }
            }

            return sortedEntities;
        }

        private static IEnumerable<ErrorKeyAndMessage> GetEntityValidationErrorKeyAndMessages(DbEntityValidationException ex, TEntity entity)
        {
            var errorKeyAndMessages = new List<ErrorKeyAndMessage>();

            var entityValidationResult = ex.EntityValidationErrors.SingleOrDefault(vr => vr.Entry.Entity == entity);
            if (entityValidationResult != null)
            {
                foreach (var validationError in entityValidationResult.ValidationErrors)
                {
                    // We want to display some of our error messages with HTML, so make sure to HTML encode the user input
                    string errorMessage = HttpUtility.HtmlEncode(validationError.ErrorMessage);
                    errorKeyAndMessages.Add(new ErrorKeyAndMessage(validationError.PropertyName, errorMessage));
                }
            }

            var otherEntityValidationResults = ex.EntityValidationErrors.Where(vr => vr.Entry.Entity != entity);
            foreach (var validationResult in otherEntityValidationResults)
            {
                foreach (var validationError in validationResult.ValidationErrors)
                {
                    string otherEntityDescription = validationResult.Entry.Entity.GetType().Description();

                    // We want to display some of our error messages with HTML, so make sure to HTML encode the user input
                    string errorMessage = HttpUtility.HtmlEncode(validationError.ErrorMessage);
                    string error = !string.IsNullOrEmpty(validationError.PropertyName)
                                       ? @"{0}&rsquo;s {1}: {2}.".F(
                                           otherEntityDescription,
                                           validationError.PropertyName,
                                           errorMessage)
                                       : @"{0}: {1}".F(otherEntityDescription, errorMessage);
                    errorKeyAndMessages.Add(new ErrorKeyAndMessage(string.Empty, error));
                }
            }

            return errorKeyAndMessages;
        }

        #endregion


        #region Nested Classes

        protected sealed class Sort : SortBase<TEntity>
        {
        }

        private class ErrorKeyAndMessage
        {
            public readonly string ErrorKey;

            public readonly string ErrorMessage;

            public ErrorKeyAndMessage(string errorKey, string errorMessage)
            {
                this.ErrorKey = errorKey;
                this.ErrorMessage = errorMessage;
            }
        }

        #endregion
    }
}
