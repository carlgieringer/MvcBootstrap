namespace MvcBootstrap.Views.Extensions
{
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System;

    using MvcBootstrap.Properties;

    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Output an HTML submit tag.
        /// </summary>
        public static MvcHtmlString Submit<TModel>(this HtmlHelper<TModel> html, string value, object htmlAttributes = null)
        {
            var tagBuilder = new TagBuilder("input");

            tagBuilder.MergeAttribute("type", "submit");
            tagBuilder.MergeAttribute("value", value);

            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            tagBuilder.MergeAttributes(attributes);

            return tagBuilder.ToMvcHtmlString(TagRenderMode.SelfClosing);
        }

        /// <summary>
        /// Output an HTML tag with different name and value.
        /// </summary>
        public static MvcHtmlString Submit<TModel>(this HtmlHelper<TModel> html, string name, string value, object htmlAttributes = null)
        {
            var tagBuilder = new TagBuilder("input");

            tagBuilder.MergeAttribute("type", "submit");
            tagBuilder.MergeAttribute("name", name);
            tagBuilder.MergeAttribute("value", value);

            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            tagBuilder.MergeAttributes(attributes);

            return tagBuilder.ToMvcHtmlString(TagRenderMode.SelfClosing);
        }

        public static MvcHtmlString ActionLinkAbsolute<TModel>(
            this HtmlHelper<TModel> html,
            string linkText,
            [AspMvcAction] string actionName,
            [AspMvcController] string controllerName,
            object routeValues = null,
            object htmlAttributes = null)
        {
            var request = html.ViewContext.HttpContext.Request;
            var url = new UriBuilder(request.Url);
            // If either protocol or host parameters to HtmlHelper (or UrlHelper) are non-empty, they generate an absolute URL.
            return html.ActionLink(linkText, actionName, controllerName, url.Scheme, url.Host, null, routeValues, htmlAttributes);
        }

        public static string ControllerName<TModel>(this HtmlHelper<TModel> html)
        {
            return html.ViewContext.Controller.ControllerContext.RouteData.Values["controller"].ToString();
        }
    }
}