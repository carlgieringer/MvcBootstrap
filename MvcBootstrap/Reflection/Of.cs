namespace TEMTDomain.StaticLib
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Mvc;

    using MvcBootstrap.Util;

    public static class Of<T>
    {
        /// <remarks>
        /// The benefit of this method over <see cref="NameFor{MemberT}"/> is that it will use 
        /// <see cref="DisplayNameAttribute"/> when present.  But it currently doesn't handle multiple
        /// levels of member accesses.
        /// </remarks>
        /// <returns>
        /// Text representing the member accessed in <paramref name="memberExpression"/> suitable for display.
        /// </returns>
        public static string DisplayNameFor<MemberT>(Expression<Func<T, MemberT>> memberExpression)
        {
            // TODO combine this and NameFor
            // Get ExpressionHelper.GetExpressionText(memberExpression)
            // Split by '.'
            // Walk the chain of each, looking for DisplayName attribute
            // Use DisplayName attribute or InsertSpacesBetween words
            // Join results with spaces.

            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("memberLambdaExpression must be a lambda returning a member.");
            }

            var pi = body.Member as PropertyInfo;
            if (pi == null)
            {
                throw new ArgumentException("memberLambdaExpression must be a lambda returning a member.");
            }

            string description = null;
            var displayNameAttribute = pi.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttribute != null)
            {
                description = displayNameAttribute.DisplayName;
            }
            if (description == null)
            {
                description = StringHelper.SplitCamelCase(pi.Name);
            }

            return description;
        }

        /// <remarks>
        /// The benefit of this method over <see cref="DisplayNameFor{MemberT}"/> is that it can handle
        /// multiple levels of member accesses.  But it doesn't look at <see cref="DisplayNameAttribute"/>.
        /// </remarks>
        /// <returns>
        /// The code name of <paramref name="memberExpression"/> with periods replaced by spaces.
        /// </returns>
        public static string NameFor<MemberT>(Expression<Func<T, MemberT>> memberExpression)
        {
            // TODO combine with DisplayNameFor so that DisplayName attribute is respected if present on any member.

            var codeName = CodeNameFor(memberExpression);
            var friendlyName = StringHelper.ConvertDotNotationToSpaceDelimited(codeName);
            return friendlyName;
        }

        /// <returns>
        /// The text representation of the member accessed in <paramref name="memberExpression"/>
        /// </returns>
        public static string CodeNameFor<MemberT>(Expression<Func<T, MemberT>> memberExpression)
        {
            return ExpressionHelper.GetExpressionText(memberExpression);
        }
    }
}
