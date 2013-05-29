namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System.Collections.Generic;
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

        public readonly MappingCreator<TEntity> MappingCreator;

        protected readonly IBootstrapRepository<TEntity> Repository;

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

            this.MappingCreator = new MappingCreator<TEntity>(this.Config.RelationsConfig);
            this.MappingCreator.InitializeMapping<TViewModel>(DependencyResolver.Current);
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
                return this.Config.RelationsConfig;
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
            var entities = this.Repository.GetAll();

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
            return this.View(this.Config.CreateViewName, viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
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
            return this.View(this.Config.UpdateViewName, viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
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


        #region Private Methods

        private static IEnumerable<ErrorKeyAndMessage> GetEntityValidationErrorKeyAndMessages(DbEntityValidationException ex, TEntity entity)
        {
            var errorKeyAndMessages = new List<ErrorKeyAndMessage>();

            var entityValidationResult = ex.EntityValidationErrors.SingleOrDefault(vr => vr.Entry.Entity == entity);
            if (entityValidationResult != null)
            {
                foreach (var validationError in entityValidationResult.ValidationErrors)
                {
                    // We want to be able to include HTML in our error messages, so encode the validation error message which
                    // may include user input
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

                    // We want to be able to include HTML in our error messages, so encode the validation error message which
                    // may include user input
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
