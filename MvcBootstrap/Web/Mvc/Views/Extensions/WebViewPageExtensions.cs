namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.Routing;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;

    public static class WebViewPageExtensions
    {
        public static string ControllerName<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewContext.Controller.ControllerContext.RouteData.Values["controller"].ToString();
        }

        public static bool HasFlash<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewData.ContainsKey(ControllerExtensions.FlashMessageKey) ||
                page.TempData.ContainsKey(ControllerExtensions.FlashMessageKey);
        }

        public static string FlashMessage<TModel>(this WebViewPage<TModel> page)
        {
            return (string)(page.ViewData[ControllerExtensions.FlashMessageKey] ??
                page.TempData[ControllerExtensions.FlashMessageKey]);
        }

        public static FlashKind FlashKind<TModel>(this WebViewPage<TModel> page)
        {
            return (FlashKind)(page.ViewData[ControllerExtensions.FlashKindKey] ??
                page.TempData[ControllerExtensions.FlashKindKey]);
        }

        public static string FlashAlertClass<TModel>(this WebViewPage<TModel> page)
        {
            return page.Html.AttributeEncode(page.FlashKind().ToAlertClass());
        }

        public static string ActionName<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewContext.Controller.ControllerContext.RouteData.Values["action"].ToString();
        }

        public static RouteValueDictionary RouteValues<TModel>(this WebViewPage<TModel> page)
        {
            return page.ViewContext.Controller.ControllerContext.RouteData.Values;
        }

        public static MvcHtmlString EntityActionTitle<TModel>(this WebViewPage<TModel> page)
            where TModel : class, IEntityViewModel
        {
            var args = new List<object>();
            
            args.Add(page.ControllerName());

            string label = null;
            if (page.Model != null)
            {
                var controllerAsSelectorContainer = page.ViewContext.Controller as IViewModelLabelSelectorContainer<TModel>;
                if (controllerAsSelectorContainer != null)
                {
                    label = controllerAsSelectorContainer.LabelSelector.ViewModelLabelSelector(page.Model);
                }

                if (string.IsNullOrWhiteSpace(label) && page.Model.Id.HasValue)
                {
                    args.Add(page.Model.Id.Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(label))
                {
                    args.Add(label);
                }
            }

            var actionName = page.ActionName();
            if (actionName != "Index")
            {
                args.Add(actionName);
            }

            return MvcHtmlString.Create(string.Join(" &rsaquo; ", args));
        }

        public static Type EntityType<TModel>(this WebViewPage<TModel> page)
        {
            var controllerType = page.ViewContext.Controller.GetType();
            if (controllerType.IsAssignableTo(typeof(BootstrapControllerBase<,>)))
            {
                return controllerType.GenericTypeArguments[0];
            }

            return null;
        }


        public static BootstrapViewHelper<TModel> Bootstrap<TModel>(this WebViewPage<TModel> page)
        {
            return new BootstrapViewHelper<TModel>(page);
        }
    }
}
