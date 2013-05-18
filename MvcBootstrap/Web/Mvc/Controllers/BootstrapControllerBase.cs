namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using AutoMapper;

    using MvcBootstrap.Data;
    using MvcBootstrap.Exceptions;
    using MvcBootstrap.Extensions;
    using MvcBootstrap.Mapping;
    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;

    public abstract class BootstrapControllerBase<TEntity, TViewModel> : Controller, IViewModelLabelSelectorContainer<TViewModel>
        where TEntity : class, IEntity
        where TViewModel : class, IEntityViewModel
    {
        protected readonly IBootstrapRepository<TEntity> Repository;

        protected readonly IMappingExpression<TEntity, TViewModel> EntityToViewModelMappingExpression;

        protected readonly IMappingExpression<TViewModel, TEntity> ViewModelToEntityMappingExpression;

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

            this.EntityToViewModelMappingExpression = MappingHelper.CreateEntityToViewModelMap<TEntity, TViewModel>();
            this.ViewModelToEntityMappingExpression = MappingHelper.CreateViewModelToEntityMap<TViewModel, TEntity>();
        }

        public IViewModelLabelSelector<TViewModel> LabelSelector
        {
            get { return this.Config; }
        } 

        public BootstrapControllerConfig<TEntity, TViewModel> Config { get; private set; }

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

                this.Flash(@"{0} ""{1}"" Created".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)), FlashKind.Success);
                
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

            if (this.ModelState.IsValid)
            {
                var entity = this.Repository.GetById(viewModel.Id.Value);
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

                this.Flash(@"{0} ""{1}"" Updated".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)), FlashKind.Success);

                return this.RedirectToAction("List");
            }

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

        protected sealed class Sort : SortBase<TEntity>
        {

        }
    }
}
