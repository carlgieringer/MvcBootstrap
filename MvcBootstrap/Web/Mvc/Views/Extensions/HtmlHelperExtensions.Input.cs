namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using MvcBootstrap.Extensions;

    /// <summary>
    /// Again I refactored, to rely upon the MS helpers to construct the inputs myself, so the following may not be 100% accurate since I was
    /// able combine the form that allows a value with the form that
    /// 
    /// There are two types of built-in HTML input helpers: one that accepts a string for the name (e.g., Html.Hidden(...), and a strongly typed one 
    /// that accepts an expression resulting in the view model member which provides the name of the input (e.g., Html.HiddenFor(...).  
    /// The problem with the former is that it provides no static check that the member actually exists on a view model, and the problem with the 
    /// latter is that it does not allow us to change the initial value of the input.
    /// 
    /// The extension methods in this class allow us to change the intial value of the input, and also to create a statically checked input for a 
    /// member of a view model that is not the same as the page's view model if we provide the types of the model and member.
    /// 
    /// ACTUALLY it now appears to me that MVC's built-in generic HTML helpers are braindamaged.  Not only do they override the value we pass 
    /// (at least for HiddenWith) but they also set the id of the input to be the name of the member on the viewmodel.  That means that if for some reason
    /// you see fit to have two forms on your page for the same viewmodel, they will have conflicting IDs.  What a bright idea.  I guess this is
    /// part of MS's enforcing a pattern where a view model has a 1-to-1 relation with the view.
    /// 
    /// We get around the overwritten value thing in *With by casting to a non-generic HtmlHelper.  Genius MVC still sets the ID, though.
    /// </summary>
    public static class HtmlHelperInputExtensions
    {
        /// <summary>
        /// Creates an html input tag with type attribute = <paramref name="type"/>.
        /// </summary>
        public static MvcHtmlString InputWith<TViewModel, TMember>(
            this HtmlHelper html,
            string type,
            Expression<Func<TViewModel, TMember>> memberExpression,
            object value = null,
            object htmlAttributes = null)
        {
            var memberName = ExpressionHelper.GetExpressionText(memberExpression);

            var builder = new TagBuilder("input");
            builder.Attributes["type"] = type;
            builder.Attributes["name"] = memberName;
            builder.Attributes["value"] = value != null ? value.ToString() : "";
            // Now merge any attributes sent via the anonymous object; this allows overwrite of the attributes
            // set above.  Caveat Programmer.
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

        #region hidden

        public static MvcHtmlString HiddenWith<TViewModel, TMember>(
            this HtmlHelper<TViewModel> html,
            Expression<Func<TViewModel, TMember>> memberExpression,
            object value = null,
            object htmlAttributes = null)
        {
            return html.InputWith("hidden", memberExpression, value, htmlAttributes);
        }

        public static MvcHtmlString HiddenWith<TViewModel, TMember>(
            this HtmlHelper html,
            Expression<Func<TViewModel, TMember>> memberExpression,
            object value = null,
            object htmlAttributes = null)
        {
            return html.InputWith("hidden", memberExpression, value, htmlAttributes);
        }

        #endregion


        #region textbox

        public static MvcHtmlString TextBoxWith<TViewModel, TMember>(
            this HtmlHelper<TViewModel> html,
            Expression<Func<TViewModel, TMember>> memberExpression,
            object value = null,
            object htmlAttributes = null)
        {
            return html.InputWith("text", memberExpression, value, htmlAttributes);
        }

        public static MvcHtmlString TextBoxWith<TViewModel, TMember>(
            this HtmlHelper html,
            Expression<Func<TViewModel, TMember>> memberExpression,
            object value = null,
            object htmlAttributes = null)
        {
            return html.InputWith("text", memberExpression, value, htmlAttributes);
        }

        #endregion


        #region checkbox

        public static MvcHtmlString CheckBoxWith<TViewModel, TMember>(
            this HtmlHelper<TViewModel> html,
            Expression<Func<TViewModel, TMember>> memberExpression,
            bool isChecked,
            object htmlAttributes)
        {
            var memberName = ExpressionHelper.GetExpressionText(memberExpression);

            var builder = new TagBuilder("input");
            builder.Attributes["type"] = "checkbox";
            builder.Attributes["name"] = memberName;
            if (isChecked)
            {
                builder.Attributes["checked"] = "checked";
            }
            // Now merge any attributes sent via the anonymous object; this allows overwrite of the attributes
            // set above.  Caveat Programmer.
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString CheckBoxWith<TViewModel, TMember>(
            this HtmlHelper html,
            Expression<Func<TViewModel, TMember>> memberExpression,
            bool isChecked,
            object htmlAttributes)
        {
            var memberName = ExpressionHelper.GetExpressionText(memberExpression);

            var builder = new TagBuilder("input");
            builder.Attributes["type"] = "checkbox";
            builder.Attributes["name"] = memberName;
            if (isChecked)
            {
                builder.Attributes["checked"] = "checked";
            }
            // Now merge any attributes sent via the anonymous object; this allows overwrite of the attributes
            // set above.  Caveat Programmer.
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

        #endregion


        public static MvcHtmlString LabelWith<TViewModel, TMember>(
            this HtmlHelper<TViewModel> html,
            Expression<Func<TViewModel, TMember>> memberExpression,
            string labelText = null,
            object htmlAttributes = null)
        {
            var memberName = ExpressionHelper.GetExpressionText(memberExpression);
            // The generic HtmlHelper will try to overwrite our value.
            var plainHtml = (HtmlHelper)html;
            return plainHtml.Label(memberName, labelText, htmlAttributes);
        }
    }
}