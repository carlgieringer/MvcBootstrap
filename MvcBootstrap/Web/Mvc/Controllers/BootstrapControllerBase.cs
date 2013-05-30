namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
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

        public IViewModelLabelSelectorOwner<TViewModel> ViewModelLabelSelectorOwner
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
                try
                {
                    this.Repository.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    this.ModelState.AddModelError(
                        string.Empty,
                        @"Unable to create the {0}:".F(typeof(TEntity).Description()));

                    var errorKeyAndMessages = GetEntityValidationErrorKeyAndMessages(ex, entity);
                    foreach (var errorKeyAndMessage in errorKeyAndMessages)
                    {
                        this.ModelState.AddModelError(errorKeyAndMessage.ErrorKey, errorKeyAndMessage.ErrorMessage);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    this.Flash(
                        @"Created {0} ""{1}""".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)),
                        FlashKind.Success);

                    return this.RedirectToAction("List");
                }
            }
            else
            {
                this.ModelState.AddModelError(
                    string.Empty,
                    @"Unable to create the {0}:".F(typeof(TEntity).Description()));
            }

            viewModel = this.RefreshViewModel(viewModel);

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

                    const string ErrorMessageFormat = "Someone edited the {0} before you saved.  "
                                             + "Please review the new values below next to yours.  "
                                             + "Cancel to leave the new current values.";
                    this.ModelState.AddModelError(
                        string.Empty, 
                        string.Format(ErrorMessageFormat, typeof(TViewModel).Description()));
                }
                catch (DbEntityValidationException ex)
                {
                    this.ModelState.AddModelError(
                        string.Empty,
                        @"Unable to update the {0}:".F(typeof(TEntity).Description()));

                    var errorKeyAndMessages = GetEntityValidationErrorKeyAndMessages(ex, entity);
                    foreach (var errorKeyAndMessage in errorKeyAndMessages)
                    {
                        this.ModelState.AddModelError(errorKeyAndMessage.ErrorKey, errorKeyAndMessage.ErrorMessage);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    this.Flash(
                        @"Updated {0} ""{1}""".F(typeof(TEntity).Description(), this.Config.EntityLabelSelector(entity)),
                        FlashKind.Success);

                    return this.RedirectToAction("List");
                }
            }
            else
            {
                this.ModelState.AddModelError(
                    string.Empty,
                    @"Unable to update the {0}:".F(typeof(TEntity).Description()));
            }

            viewModel = this.RefreshViewModel(viewModel);

            // Since we got here, there was some validation problem.  So load the OriginalValues.
            // Reset the entity before mapping it because it will have altered values from the viewmodel mapping
            // above or even the RefreshViewModel can affect another entity.
            entity = this.Repository.Reset(entity);
            viewModel.OriginalValues = Mapper.Map<TViewModel>(entity);

            return this.View(this.Config.UpdateViewName, viewModel);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var entity = this.Repository.GetById(id);
            if (entity == null)
            {
                return this.HttpNotFound();
            }

            var viewModel = Mapper.Map<TViewModel>(entity);
            return this.View(this.Config.DeleteViewName, viewModel);
        }

        [HttpPost]
        public ActionResult Delete(TViewModel viewModel)
        {
            if (!viewModel.Id.HasValue)
            {
                return this.HttpNotFound();
            }

            var entity = this.Repository.GetById(viewModel.Id.Value);
            if (entity == null)
            {
                return this.HttpNotFound();
            }

            var entityLabel = this.Config.EntityLabelSelector(entity);

            this.Repository.Delete(entity);
            try
            {
                this.Repository.SaveChanges();
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError(
                    string.Empty,
                    @"Unable to delete the {0}: {1}".F(typeof(TEntity).Description(), ex.Message));
            }

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

        /// <summary>
        /// Performs a mapping to <paramref name="viewModel"/> to ensure that it properties are set correctly for
        /// re-display.
        /// </summary>
        /// <remarks>
        /// <see cref="Choice{T}.Options"/> and <see cref="Choices{T}.Options"/> will not have been set by model
        /// binding because the model binder does not know <typeparamref name="TEntity"/> in order to retrieve
        /// an instance of <typeparamref name="TEntity"/> from a repository, and the choice options can only
        /// be set from knowing the instance.
        /// </remarks>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private TViewModel RefreshViewModel(TViewModel viewModel)
        {
            // Start with a fresh entity...
            var freshEntity = viewModel.Id.HasValue ?
                this.Repository.GetById(viewModel.Id.Value) :
                this.Repository.Create();

            // Map the view model changes to it...
            freshEntity = Mapper.Map(viewModel, freshEntity);

            // And then map back to the viewModel to set the choice options.
            viewModel = Mapper.Map(freshEntity, viewModel);

            return viewModel;
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
