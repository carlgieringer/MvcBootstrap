namespace TEMTDomain.StaticLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Mvc;

    using System.Text.RegularExpressions;

    public static class Of<T>
    {
        // ReSharper disable StaticFieldInGenericType
        internal static readonly Regex CamelCaseWithAcronymsWordRegex = new Regex(@"(\B[A-Z][^A-Z]+)|\B(?<=[^A-Z]+)([A-Z]+)(?![^A-Z])");
        
        internal static readonly string[] TitleCaseLowerCaseWords = new[]
            {
                "Or", "And", "Of", "On", "The", "For", "At", "A", "In", "By", "About", "To", "From", "With", "Over", "Into", "Without"
            };

        private static readonly Dictionary<string, string> TitleCaseeLowerCaseWordsSpacedToLower; 
        // ReSharper restore StaticFieldInGenericType

        static Of()
        {
            TitleCaseeLowerCaseWordsSpacedToLower = new Dictionary<string, string>();
            foreach (var word in TitleCaseLowerCaseWords)
            {
                TitleCaseeLowerCaseWordsSpacedToLower.Add(" " + word + " ", " " + word.ToLower() + " ");
            }
        }

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
                description = InsertSpacesBetweenCamelCaseWords(pi.Name);
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

            return CodeNameFor(memberExpression).Replace('.', ' ');
        }

        /// <returns>
        /// The text representation of the member accessed in <paramref name="memberExpression"/>
        /// </returns>
        public static string CodeNameFor<MemberT>(Expression<Func<T, MemberT>> memberExpression)
        {
            return ExpressionHelper.GetExpressionText(memberExpression);
        }

        public static string InsertSpacesBetweenCamelCaseWords(string str)
        {
            var result = CamelCaseWithAcronymsWordRegex.Replace(str, " $1$2");
            foreach (var kv in TitleCaseeLowerCaseWordsSpacedToLower)
            {
                result = result.Replace(kv.Key, kv.Value);
            }

            return result;
        }
    }
}
