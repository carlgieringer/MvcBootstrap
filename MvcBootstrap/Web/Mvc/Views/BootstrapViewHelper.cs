namespace MvcBootstrap.Web.Mvc.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Util;
    using MvcBootstrap.ViewModels;
    using MvcBootstrap.Web.Mvc.Controllers;
    using MvcBootstrap.Web.Mvc.Views.Extensions;

    /// <summary>
    /// Provides an extension method for obtaining a <see cref="BootstrapViewHelper{TModel}"/> from a
    /// <see cref="WebViewPage{TModel}"/>.
    /// </summary>
    public static class WebViewPageBootstrapExtensions
    {
        /// <summary>
        /// Returns a new  <see cref="BootstrapViewHelper{TModel}"/> based upon <paramref name="page"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="page"></param>
        /// <returns></returns>
        public static BootstrapViewHelper<TModel> Bootstrap<TModel>(this WebViewPage<TModel> page) where TModel : class
        {
            return new BootstrapViewHelper<TModel>(page);
        }
    }

    /// <summary>
    /// Provides methods that assist in writing views when using <see cref="IBootstrapController{TViewModel}"/> and 
    /// Twitter Bootstrap.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class BootstrapViewHelper<TModel>
        where TModel : class
    {
        #region Fields

        private readonly WebViewPage<TModel> page;

        #endregion


        #region Constructors

        public BootstrapViewHelper(WebViewPage<TModel> page)
        {
            this.page = page;
        }

        #endregion


        #region Label Methods

        public MvcHtmlString ViewModelLabelFor<TFor>(string propertyName, Expression<Func<TModel, TFor>> expression)
            where TFor : IEntityViewModel
        {
            var viewModel = expression.Compile()(this.page.Model);

            string label;

            var controllerContainer = this.page.ViewContext.Controller as IRelatedViewModelLabelSelectorContainer;
            if (controllerContainer != null)
            {
                label = controllerContainer.RelatedViewModelLabelSelector.GetRelatedViewModelLabel(
                    propertyName, viewModel);
            }
            else
            {
                label = viewModel.ToString();
            }

            return MvcHtmlString.Create(label);
        }

        public MvcHtmlString ViewModelLabelForModel()
        {
            string label;

            var controllerAsContainer =
                this.page.ViewContext.Controller as IViewModelLabelSelectorContainer<IEntityViewModel>;
            var modelAsEntityViewModel = this.page.Model as IEntityViewModel;
            if (controllerAsContainer != null && modelAsEntityViewModel != null)
            {
                label = controllerAsContainer.ViewModelLabelSelectorOwner.ViewModelLabelSelector(modelAsEntityViewModel);
            }
            else
            {
                label = this.page.Model.ToString();
            }

            return MvcHtmlString.Create(label);
        }

        #endregion


        #region Breadcrumb Methods

        /// <summary>
        /// Creates HTML describing a hierachical path for the current page in the applistion.
        /// </summary>
        /// <returns>
        /// An <see cref="MvcHtmlString"/> containing an unordered list styled as Twitter Bootstrap
        /// breadcrumbs.
        /// </returns>
        public MvcHtmlString Breadcrumbs()
        {
            var breadcrumbs = this.page.GetBreadcrumbsList();

            var dividerBuilder = new TagBuilder(HtmlTag.Span);
            dividerBuilder.AddCssClass(BootstrapCss.Divider);
            dividerBuilder.SetInnerText("/");
            string dividerString = dividerBuilder.ToString();

            var listItemBuilders = new List<TagBuilder>();

            var anchorBuilder = new TagBuilder(HtmlTag.A);
            // Breadcrumbs other than the last one are hyperlinked
            var ancestorCrumbs = breadcrumbs.Take(breadcrumbs.Count() - 1);
            foreach (var breadcrumb in ancestorCrumbs)
            {
                anchorBuilder.Attributes["href"] = breadcrumb.Path;
                anchorBuilder.SetInnerText(breadcrumb.Text);

                var listItemBuilder = new TagBuilder(HtmlTag.LI);
                listItemBuilder.InnerHtml = anchorBuilder + dividerString;
                listItemBuilders.Add(listItemBuilder);
            }

            var lastCrumb = breadcrumbs.Last();
            var lastListItemBuilder = new TagBuilder(HtmlTag.LI);
            lastListItemBuilder.AddCssClass(BootstrapCss.Active);
            lastListItemBuilder.SetInnerText(lastCrumb.Text);
            listItemBuilders.Add(lastListItemBuilder);


            var listBuilder = new TagBuilder(HtmlTag.UL);
            listBuilder.AddCssClass(BootstrapCss.Breadcrumb);
            listBuilder.InnerHtml = string.Join(string.Empty, listItemBuilders.Select(b => b.ToString()));

            return listBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        #endregion


        #region Form Methods

        public MvcForm BeginCreateForm()
        {
            string createAction = "Create";
            return this.page.Html.BeginForm(
                createAction, this.page.Html.ControllerName(), FormMethod.Post, new { @class = "form-horizontal" });
        }

        public MvcForm BeginUpdateForm()
        {
            string updateAction = "Update";
            return this.page.Html.BeginForm(
                updateAction, this.page.Html.ControllerName(), FormMethod.Post, new { @class = "form-horizontal" });
        }

        public MvcForm BeginForm()
        {
            return this.BeginForm(null, null, new { });
        }

        public MvcForm BeginForm(string actionName, string controllerName, object htmlAttributes)
        {
            var attributeDict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            object classAttribute;
            classAttribute = attributeDict.TryGetValue("class", out classAttribute)
                ? string.Format("{0} {1}", classAttribute, "form-horizontal")
                : "form-horizontal";

            attributeDict["class"] = classAttribute;

            return this.page.Html.BeginForm(actionName, controllerName, FormMethod.Post, attributeDict);
        }

        public MvcForm BeginForm(object htmlAttributes)
        {
            return this.BeginForm(null, null, htmlAttributes);
        }

        public MvcForm BeginForm(string actionName, string controllerName)
        {
            return this.BeginForm(actionName, controllerName, null);
        }

        #endregion


        #region Link Methods

        public MvcHtmlString ReturnLink(string format = "&laquo; Back to {0}", object htmlAttributes = null)
        {
            string returnAction = "Index";
            var builder = new TagBuilder("a");
            builder.InnerHtml = format.F(this.page.ControllerName());
            builder.Attributes["href"] = this.page.Url.Action(returnAction, new { id = (int?)null });
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString());
        }

        public MvcHtmlString CreateLink(string text = "Create", Icon icon = Icon.Plus, ButtonStyle style = ButtonStyle.Primary)
        {
            var linkBuilder = new TagBuilder("a");
            linkBuilder.Attributes["href"] = page.Url.Action(BootstrapActionName.Create, this.page.ControllerName());

            FormatTag(linkBuilder, text, icon, style);

            return linkBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString ReadLink(int id, string text = "View", Icon icon = Icon.None, ButtonStyle style = ButtonStyle.Default)
        {
            var linkBuilder = new TagBuilder("a");
            linkBuilder.Attributes["href"] = page.Url.Action(BootstrapActionName.Read, this.page.ControllerName(), new { id });

            FormatTag(linkBuilder, text, icon, style);

            return linkBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString UpdateLink(int id, string text = "Edit", Icon icon = Icon.Edit, ButtonStyle style = ButtonStyle.Default)
        {
            var linkBuilder = new TagBuilder("a");
            linkBuilder.Attributes["href"] = page.Url.Action(BootstrapActionName.Update, this.page.ControllerName(), new { id });

            FormatTag(linkBuilder, text, icon, style);

            return linkBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString CancelLink(string text = "Cancel", Icon icon = Icon.None, ButtonStyle style = ButtonStyle.Default)
        {
            var linkBuilder = new TagBuilder("a");
            linkBuilder.Attributes["href"] = page.Url.Action(BootstrapActionName.List, this.page.ControllerName());

            FormatTag(linkBuilder, text, icon, style);

            return linkBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString CancelLink(int? id, string text = "Cancel", Icon icon = Icon.None, ButtonStyle style = ButtonStyle.Default)
        {
            var linkBuilder = new TagBuilder("a");
            linkBuilder.Attributes["href"] = page.Url.Action(BootstrapActionName.Read, this.page.ControllerName(), new { id });

            FormatTag(linkBuilder, text, icon, style);

            return linkBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString DeleteLink(int id, string text = "Delete", Icon icon = Icon.None, ButtonStyle style = ButtonStyle.Default)
        {
            var linkBuilder = new TagBuilder("a");
            linkBuilder.Attributes["href"] = page.Url.Action(BootstrapActionName.Delete, this.page.ControllerName(), new { id });

            FormatTag(linkBuilder, text, icon, style);

            return linkBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString ActionLink<TController>(string linkText, Expression<Func<TController, ActionResult>> action, object htmlAttributes = null)
            where TController : IController
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

        #endregion


        #region Form Input Methods

        public MvcHtmlString Submit(string text, Icon icon = Icon.None, ButtonStyle style = ButtonStyle.Default, string title = null)
        {
            var buttonBuilder = new TagBuilder("button");

            if (!string.IsNullOrEmpty(title))
            {
                buttonBuilder.Attributes["title"] = title;
            }

            FormatTag(buttonBuilder, text, icon, style);

            return buttonBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString LabelFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            return this.page.Html.LabelFor(expression, new { @class = "control-label" });
        }

        public MvcHtmlString TextBoxFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("controls");
            tagBuilder.InnerHtml = this.page.Html.TextBoxFor(expression).ToHtmlString() +
                this.page.Html.ValidationMessageFor(expression, null, new { @class = "text-error" }).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString PasswordFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("controls");
            tagBuilder.InnerHtml = this.page.Html.PasswordFor(expression).ToHtmlString() +
                this.ValidationMessageFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString TextBoxGroupFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("control-group");
            tagBuilder.InnerHtml = this.LabelFor(expression).ToHtmlString() + this.TextBoxFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString SubmitGroup(string text, Icon icon = Icon.None, ButtonStyle style = ButtonStyle.Primary)
        {
            var controlsBuilder = new TagBuilder("div");
            controlsBuilder.AddCssClass("controls");
            controlsBuilder.InnerHtml = this.Submit(text, icon, style).ToHtmlString();

            var groupBuilder = new TagBuilder("div");
            groupBuilder.AddCssClass("control-group");
            groupBuilder.InnerHtml = controlsBuilder.ToString();

            return groupBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString PasswordGroupFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("control-group");
            tagBuilder.InnerHtml = this.LabelFor(expression).ToHtmlString() +
                this.PasswordFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString CheckboxGroupFor(Expression<Func<TModel, bool>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("control-group");
            tagBuilder.InnerHtml = this.LabelFor(expression).ToHtmlString() +
                this.CheckboxFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        public MvcHtmlString CheckboxFor(Expression<Func<TModel, bool>> expression)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("controls");
            tagBuilder.InnerHtml = this.page.Html.CheckBoxFor(expression).ToHtmlString() +
                this.ValidationMessageFor(expression).ToHtmlString();
            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }

        #endregion


        #region Validation Methods

        public IHtmlString ValidationSummary(string message = null)
        {
            var validationSummary = this.page.Html.ValidationSummary(true, message, new { @class = "text-error" }) ??
                MvcHtmlString.Empty;
            return this.page.Html.Raw(HttpUtility.HtmlDecode(validationSummary.ToHtmlString()));
        }

        public MvcHtmlString ValidationMessageFor<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            return this.page.Html.ValidationMessageFor(expression, null, new { @class = "text-error" });
        }

        public MvcHtmlString ValidationMessage(string propertyName)
        {
            return this.page.Html.ValidationMessage(propertyName, null, new { @class = "text-error" });
        }

        #endregion


        #region Helper Methods

        private static void FormatTag(TagBuilder tagBuilder, string text, Icon icon, ButtonStyle style)
        {
            switch (style)
            {
                case ButtonStyle.None:
                    // No button formatting
                    break;
                case ButtonStyle.Default:
                    tagBuilder.AddCssClass("btn");
                    break;
                default:
                    tagBuilder.AddCssClass("btn");
                    tagBuilder.AddCssClass(ToCssClass(style));
                    break;
            }

            if (icon != Icon.None)
            {
                var iconBuilder = new TagBuilder("i");
                iconBuilder.AddCssClass(ToCssClass(icon));
                if (IsDark(style))
                {
                    iconBuilder.AddCssClass("icon-white");
                }

                // Add a space to keep the icon and text from running together
                tagBuilder.InnerHtml += iconBuilder + " ";
            }

            tagBuilder.InnerHtml += text;
        }

        private static string ToCssClass(Icon icon)
        {
            return string.Format("icon-{0}", StringHelper.SplitCamelCase(icon.ToString(), "-").ToLower());
        }

        private static string ToCssClass(ButtonStyle style)
        {
            return string.Format("btn-{0}", StringHelper.SplitCamelCase(style.ToString(), "-").ToLower());
        }

        private static string ToCssClass(TextEmphasis emphasis)
        {
            return string.Format("text-{0}", StringHelper.SplitCamelCase(emphasis.ToString(), "-").ToLower());
        }

        private static bool IsDark(ButtonStyle style)
        {
            switch (style)
            {
                case ButtonStyle.Danger:
                case ButtonStyle.Inverse:
                case ButtonStyle.Primary:
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}