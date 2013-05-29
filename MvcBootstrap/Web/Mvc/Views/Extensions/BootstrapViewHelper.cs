namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using System;
    using System.Linq.Expressions;
    using System.Web;
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
            return this.Submit("Save");
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

        public MvcForm BeginForm()
        {
            return this.BeginForm(null, null, new { });
        }

        public MvcForm BeginForm(string actionName, string controllerName, object htmlAttributes)
        {
            var attributeDict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            object classAttribute;
            classAttribute = attributeDict.TryGetValue("class", out classAttribute) ? 
                string.Format("{0} {1}", classAttribute, "form-horizontal") : 
                "form-horizontal";

            attributeDict["class"] = classAttribute;

            return this.page.Html.BeginForm(actionName, controllerName, FormMethod.Post, attributeDict);
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

        public MvcHtmlString Submit(string value)
        {
            return this.page.Html.Submit(value, new { @class = "btn btn-primary" });
        }

        public MvcHtmlString LabelFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            return this.page.Html.LabelFor(expression, new { @class = "control-label" });
        }

        public MvcHtmlString TextBoxFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("controls");
            tagBuilder.InnerHtml = 
                this.page.Html.TextBoxFor(expression).ToHtmlString() + 
                this.page.Html.ValidationMessageFor(expression, null, new { @class = "text-error" }).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString PasswordFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("controls");
            tagBuilder.InnerHtml =
                this.page.Html.PasswordFor(expression).ToHtmlString() +
                this.ValidationMessageFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString ValidationMessageFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            return this.page.Html.ValidationMessageFor(expression, null, new { @class = "text-error" });
        }

        public MvcHtmlString TextBoxGroupFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("control-group");
            tagBuilder.InnerHtml = 
                this.LabelFor(expression).ToHtmlString() + 
                this.TextBoxFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString SubmitGroup(string value)
        {
            var controlsBuilder = new TagBuilder("div");
            controlsBuilder.AddCssClass("controls");
            controlsBuilder.InnerHtml = this.Submit(value).ToHtmlString();
            
            var groupBuilder = new TagBuilder("div");
            groupBuilder.AddCssClass("control-group");
            groupBuilder.InnerHtml = controlsBuilder.ToString();

            return groupBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public IHtmlString ValidationSummary(string message = null)
        {
            var validationSummary = this.page.Html.ValidationSummary(true, message, new { @class = "text-error" }) ?? MvcHtmlString.Empty;
            return this.page.Html.Raw(HttpUtility.HtmlDecode(validationSummary.ToHtmlString()));
        }

        public MvcHtmlString PasswordGroupFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("control-group");
            tagBuilder.InnerHtml =
                this.LabelFor(expression).ToHtmlString() +
                this.PasswordFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString CheckboxGroupFor(Expression<Func<TModel, bool>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("control-group");
            tagBuilder.InnerHtml =
                this.LabelFor(expression).ToHtmlString() +
                this.CheckboxFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString CheckboxFor(Expression<Func<TModel, bool>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("controls");
            tagBuilder.InnerHtml =
                this.page.Html.CheckBoxFor(expression).ToHtmlString() +
                this.ValidationMessageFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcForm BeginForm(object htmlAttributes)
        {
            return this.BeginForm(null, null, htmlAttributes);
        }

        public MvcForm BeginForm(string actionName, string controllerName)
        {
            return this.BeginForm(actionName, controllerName, null);
        }
    }
}