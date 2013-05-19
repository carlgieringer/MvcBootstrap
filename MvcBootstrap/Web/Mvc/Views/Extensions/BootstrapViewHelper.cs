namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers;

    public class BootstrapViewHelper<TModel>
    {
        private readonly WebViewPage<TModel> page;

        public BootstrapViewHelper(WebViewPage<TModel> page)
        {
            this.page = page;
        }
        
        public MvcHtmlString ViewModelLabelFor<TFor>(string propertyName, Expression<Func<TModel, TFor>> expression)
            where TFor : IEntityViewModel
        {
            var viewModel = expression.Compile()(this.page.Model);

            string label;

            var controllerAsSelectorContainer = this.page.ViewContext.Controller as IRelatedViewModelLabelSelectorContainer;
            if (controllerAsSelectorContainer != null)
            {
                label = controllerAsSelectorContainer.RelatedViewModelLabelSelector.GetRelatedViewModelLabel(propertyName, viewModel);
            }
            else
            {
                label = viewModel.ToString();
            }

            return MvcHtmlString.Create(label);
        }

        public MvcHtmlString ReturnLink(string format = "&laquo; Back to {0}", object htmlAttributes = null)
        {            
            string returnAction = "Index";
            var builder = new TagBuilder("a");
            builder.InnerHtml = format.F(this.page.ControllerName());
            builder.Attributes["href"] = this.page.Url.Action(returnAction, new { id = (int?)null });
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString());
        }

        public MvcHtmlString SaveSubmit()
        {
            return this.page.Html.Submit("Save", new { @class = "btn btn-primary" });
        }

        public MvcForm BeginCreateForm()
        {
            string createAction = "Create";
            return this.page.Html.BeginForm(createAction, this.page.Html.ControllerName(), FormMethod.Post, new { @class = "form-horizontal" });
        }

        public MvcForm BeginUpdateForm()
        {
            string updateAction = "Update";
            return this.page.Html.BeginForm(updateAction, this.page.Html.ControllerName(), FormMethod.Post, new { @class = "form-horizontal" });
        }

        public MvcHtmlString CreateLink()
        {
            string createAction = "Create";
            return this.page.Html.ActionLink("Create", createAction, null, new { @class = "btn btn-primary" });
        }

        public MvcHtmlString EditLink(int id)
        {
            string updateAction = "Update";
            return this.page.Html.ActionLink("Edit", updateAction, new { id }, new { @class = "btn btn-primary" });
        }

        public MvcHtmlString ActionLink<ControllerT>(string linkText, Expression<Func<ControllerT, ActionResult>> action, object htmlAttributes = null) where ControllerT : IController
        {
            var routeValues = new RouteValueDictionary();

            var methodExpression = action.Body as MethodCallExpression;
            if (methodExpression == null)
            {
                throw new InvalidOperationException("Expression must be a method call.");
            }

            var actionName = methodExpression.Method.Name;

            var controllerName = this.page.Html.ControllerName();
            routeValues.Add("controller", controllerName);

            var parameters = methodExpression.Method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var argumentExpression = methodExpression.Arguments[i];
                var argument = Expression.Lambda(argumentExpression).Compile().DynamicInvoke();
                routeValues.Add(param.Name, argument);
            }

            return this.page.Html.ActionLink(linkText, actionName, routeValues, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public string Pluralize<T>(string target, IEnumerable<T> enumerable)
        {
            return this.Pluralize(target, enumerable.Count());
        }

        public string Pluralize(string target, int count)
        {
            string result;
            var ps = PluralizationService.CreateService(CultureInfo.CurrentCulture);
            if (ps.IsPlural(target))
            {
                result = count != 1 ? target : ps.Singularize(target);
            }
            else
            {
                result = count == 1 ? target : ps.Pluralize(target);
            }

            return result;
        }

        public MvcHtmlString CancelLink()
        {
            string viewAction = "Index";
            return this.page.Html.ActionLink("Cancel", viewAction, null, new { @class = "btn" });
        }

        public MvcHtmlString CancelLink(int? id)
        {
            string viewAction = "Read";
            return this.page.Html.ActionLink("Cancel", viewAction, new { id }, new { @class = "btn" });
        }
    }
}