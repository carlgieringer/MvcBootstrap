namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using AutoMapper;

    using MvcBootstrap.Data;
    using MvcBootstrap.Exceptions;
    using MvcBootstrap.Extensions;
    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;

    public abstract class BootstrapControllerBase<TEntity, TViewModel> : Controller, IBootstrapController<TViewModel>
        where TEntity : class, IEntity
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
                                  EntityLabelSelector =
                                      e => e.Id.ToString(),
                                  ViewModelLabelSelector =
                                      vm => vm.Id.ToString(),
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
                Mapper.Map(viewModel, entity);
                this.Repository.Update(entity);

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
                    return this.View(this.Config.UpdateViewName, viewModel);
                }

                this.Flash(
                    @"{0} ""{1}"" Updated".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)),
                    FlashKind.Success);

                return this.RedirectToAction("List");
            }

            // I am doing this just so that my custom mapping for EntityViewModelCollection occurs
            Mapper.Map(viewModel, entity);
            Mapper.Map(entity, viewModel);

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

        protected IMappingExpression<TAnyEntity, TAnyViewModel> CreateEntityToViewModelMap<TAnyEntity, TAnyViewModel>()
            where TAnyEntity : IEntity 
            where TAnyViewModel : IEntityViewModel
        {

            return
                Mapper.CreateMap<TAnyEntity, TAnyViewModel>()
                      .ForMember(vm => vm.ConcurrentlyEdited, o => o.Ignore())
                      .ForMember(vm => vm.Id, o => o.ResolveUsing(e => e.Id == 0 ? (int?)null : e.Id));
        }

        protected IMappingExpression<TAnyViewModel, TAnyEntity> CreateViewModelToEntityMap<TAnyViewModel, TAnyEntity>()
            where TAnyEntity : IEntity 
            where TAnyViewModel : IEntityViewModel
        {
            return
                Mapper.CreateMap<TAnyViewModel, TAnyEntity>()
                      .ForMember(e => e.Created, o => o.Ignore())
                      .ForMember(e => e.Modified, o => o.Ignore());
        }

        protected IMappingExpression<ICollection<TRelatedEntity>, EntityViewModelCollection>
            CreateRelatedEntityCollectionToViewModelCollectionMap<TRelatedEntity, TRelatedViewModel>()
            where TRelatedEntity : IEntity
            where TRelatedViewModel : IEntityViewModel
        {
            var mappingExpression = Mapper.CreateMap<ICollection<TRelatedEntity>, EntityViewModelCollection>();
            mappingExpression.ConvertUsing(new EntityCollectionTypeConverter<TRelatedEntity, TRelatedViewModel>(this.Config));
            return mappingExpression;
        }

        public class EntityCollectionTypeConverter<TRelatedEntity, TRelatedViewModel> : ITypeConverter<ICollection<TRelatedEntity>, EntityViewModelCollection>
            where TRelatedViewModel : IEntityViewModel
        {
            private readonly BootstrapControllerConfig<TEntity, TViewModel> controllerConfig;

            public EntityCollectionTypeConverter(BootstrapControllerConfig<TEntity, TViewModel> controllerConfig)
            {
                this.controllerConfig = controllerConfig;
            }

            public EntityViewModelCollection Convert(ResolutionContext context)
            {
                var dest = new EntityViewModelCollection();

                IEnumerable<IEntity> relatedEntityChoices;
                Func<TEntity, IEnumerable<IEntity>> relatedEntityChoicesSelector;
                if (this.controllerConfig.RelatedEntitiesSourceSelectorByMemberName.TryGetValue(context.MemberName, out relatedEntityChoicesSelector))
                {
                    TEntity entity = context.Parent.SourceValue as TEntity;
                    relatedEntityChoices = relatedEntityChoicesSelector(entity);
                }
                else
                {
                    relatedEntityChoices = Enumerable.Empty<IEntity>();
                }

                var choices = Mapper.Map<TRelatedViewModel[]>(relatedEntityChoices);
                dest.Choices = choices as IEntityViewModel[];

                var source = context.SourceValue as ICollection<TEntity>;
                if (source != null)
                {
                    foreach (var entity in source)
                    {
                        dest.Add(Mapper.Map<TViewModel>(entity));
                    }
                }

                return dest;
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

        #endregion


        #region Nested Classes

        protected sealed class Sort : SortBase<TEntity>
        {
        }

        #endregion
    }
}
