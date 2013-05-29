namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Util;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;

    using TEMTDomain.StaticLib;

    public static class WebViewPageExtensions
    {
        #region Routing-Related Methods

        public static ControllerBase Controller<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewContext.Controller;
        }

        public static string ControllerName<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewContext.Controller.Name();
        }

        public static string ActionName<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewContext.Controller.ActionName();
        }


        public static RouteValueDictionary RouteValues<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewContext.Controller.ControllerContext.RouteData.Values;
        }

        /// <summary>
        /// Calculates a description for the current page based upon the identity
        /// and heirachy of the current controller and action.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="page"></param>
        /// <returns></returns>
        public static MvcHtmlString PageTitle<TModel>(this WebViewPage<TModel> page)
            where TModel : class, IEntityViewModel
        {
            var breadcrumbTexts = page.GetBreadcrumbsList().Select(c => c.Text);
            return MvcHtmlString.Create(string.Join(" &rsaquo; ", breadcrumbTexts));
        }

        public static IEnumerable<Breadcrumb> GetBreadcrumbsList<TModel>(this WebViewPage<TModel> page)
            where TModel : class
        {
            var crumbs = new List<Breadcrumb>();

            // Begin breadcrumbs with home
            crumbs.Add(
                new Breadcrumb(
                    StringHelper.SplitCamelCase(MvcBootstrapConfig.HomeControllerName),
                    page.Url.Action(HomeControllerAction.Index, MvcBootstrapConfig.HomeControllerName)));

            var controller = page.Controller();
            string controllerName = controller.Name();
            string actionName = page.ActionName();

            if (controller.IsHomeController())
            {
                // Only actions other than Index get a breadcrumb
                if (actionName != HomeControllerAction.Index)
                {
                    crumbs.Add(
                        new Breadcrumb(
                            StringHelper.SplitCamelCase(actionName),
                            page.Url.Action(actionName, MvcBootstrapConfig.HomeControllerName)));
                }
            }
            else
            {
                // Controllers other than home get their own breadcrumb
                crumbs.Add(new Breadcrumb(controller.Name(), page.Url.Action(BootstrapActionName.List, controllerName)));

                if (controller.IsBootstrapController())
                {
                    // Bootstrap controllers have specific rules for their CRUD actions
                    switch (page.ActionName())
                    {
                        case BootstrapActionName.List:
                            // The list action is represented by the controller name breadcrumb, already added.
                            break;

                        case BootstrapActionName.Create:
                            // The create action is represented by an additional breadcrumb on the controller
                            crumbs.Add(
                                new Breadcrumb(
                                    BootstrapActionName.Create,
                                    page.Url.Action(BootstrapActionName.Create, controllerName)));
                            break;

                        case BootstrapActionName.Read:
                            {
                                var viewModel = (IEntityViewModel)page.Model;
                                // The read action is represented by the model's label
                                crumbs.Add(
                                    new Breadcrumb(
                                        page.GetModelLabel(),
                                        page.Url.Action(BootstrapActionName.Read, controllerName, new { viewModel.Id })));

                            }
                            break;

                        case BootstrapActionName.Update:
                            {
                                var viewModel = (IEntityViewModel)page.Model;
                                // The Update action gets two breadcrumbs: the model's label...
                                crumbs.Add(
                                    new Breadcrumb(
                                        page.GetModelLabel(),
                                        page.Url.Action(BootstrapActionName.Read, controllerName, new { viewModel.Id })));
                                // ... and "Update"
                                crumbs.Add(
                                    new Breadcrumb(
                                        BootstrapActionName.Update,
                                        page.Url.Action(BootstrapActionName.Update, controllerName)));
                            }
                            break;

                        case BootstrapActionName.Delete:
                            {
                                var viewModel = (IEntityViewModel)page.Model;
                                // The Delete action gets two breadcrumbs: the model's label...
                                crumbs.Add(
                                    new Breadcrumb(
                                        page.GetModelLabel(),
                                        page.Url.Action(
                                            BootstrapActionName.Read, controllerName, new { viewModel.Id })));
                                // ... and "Delete"
                                crumbs.Add(
                                    new Breadcrumb(
                                        BootstrapActionName.Delete,
                                        page.Url.Action(BootstrapActionName.Delete, controllerName)));
                            }
                            break;
                    }
                }
                else
                {
                    // Controllers that are neither Home nor Bootstrap just add their action
                    crumbs.Add(new Breadcrumb(actionName, page.Url.Action(actionName, controllerName)));
                }
            }

            return crumbs;
        }

        /// <summary>
        /// Calculates a label representing <paramref name="page"/>'s <see cref="WebViewPage{TModel}.Model"/>
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetModelLabel<TModel>(this WebViewPage<TModel> page) where TModel : class
        {
            string label = null;
            if (page.Model != null)
            {
                var controller = page.Controller();
                var controllerType = controller.GetType();

                // First try to use BootstrapController's label selector
                if (controllerType.IsConstructedGenericTypeOfDefinition(typeof(IViewModelLabelSelectorContainer<>)))
                {
                    string ownerPropName =
                        Of<IViewModelLabelSelectorContainer<IEntityViewModel>>.CodeNameFor(
                            c => c.ViewModelLabelSelectorOwner);
                    var ownerProp = controllerType.GetProperty(ownerPropName);
                    object owner = ownerProp.GetValue(controller);

                    string selectorPropName =
                        Of<IViewModelLabelSelectorOwner<IEntityViewModel>>.CodeNameFor(o => o.ViewModelLabelSelector);
                    var selectorProp = owner.GetType().GetProperty(selectorPropName);
                    var selector = (Delegate)selectorProp.GetValue(owner);

                    var modelAsEntityViewModel = (IEntityViewModel)page.Model;

                    // If there are original values, they take precedence over edited ones.
                    modelAsEntityViewModel = modelAsEntityViewModel.OriginalValues ?? 
                        modelAsEntityViewModel;

                    label = (string)selector.DynamicInvoke(modelAsEntityViewModel);
                }
                else
                {
                    // Next try an EntityViewModel's Id
                    var modelAsEntityViewModel = page.Model as IEntityViewModel;
                    if (modelAsEntityViewModel != null)
                    {
                        label = modelAsEntityViewModel.Id.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // lastly just use .ToString()
                        label = page.Model.ToString();
                    }
                }
            }

            return label;
        }

        #endregion


        #region Flash Message Methods

        public static IEnumerable<FlashMessage> ConsumeFlashes<TModel>(this WebViewPage<TModel> page)
        {
            object flashMessagesObject;
            if (page.TempData.TryGetValue(ControllerExtensions.FlashMessagesQueueKey, out flashMessagesObject) &&
                flashMessagesObject is Queue<FlashMessage>)
            {
                var flashMessages = (Queue<FlashMessage>)flashMessagesObject;
                var flashes = flashMessages.ToArray();
                // Clear the queue so that the flashes are not displayed more than once.
                flashMessages.Clear();
                return flashes;
            }

            return Enumerable.Empty<FlashMessage>();
        }

        #endregion
    }
}
