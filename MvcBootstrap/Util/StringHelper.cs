namespace MvcBootstrap.Util
{
    using System.Collections.Generic;
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public enum Count
    {
        /// <summary>
        /// A count of one
        /// </summary>
        Singular,

        /// <summary>
        /// A count of more than one
        /// </summary>
        Plural
    }

    public static class StringHelper
    {
        private static readonly Regex CamelCaseWithAcronymsWordRegex = new Regex(@"(\B[A-Z][^A-Z]+)|\B(?<=[^A-Z]+)([A-Z]+)(?![^A-Z])");

        public static string Pluralize<T>(string target, IEnumerable<T> enumerable)
        {
            return Pluralize(target, enumerable.Count() == 1 ? Count.Singular : Count.Plural);
        }

        internal static readonly string[] TitleCaseLowerCaseWords = new[]
            {
                "Or", "And", "Of", "On", "The", "For", "At", "A", "In", "By", "About", "To", "From", "With", "Over", "Into", "Without"
            };

        private static readonly Dictionary<string, string> TitleCaseeLowerCaseWordsSpacedToLower;

        static StringHelper()
        {
            TitleCaseeLowerCaseWordsSpacedToLower = new Dictionary<string, string>();
            foreach (var word in TitleCaseLowerCaseWords)
            {
                TitleCaseeLowerCaseWordsSpacedToLower.Add(" " + word + " ", " " + word.ToLower() + " ");
            }
        }

        public static string Pluralize(string target, Count count)
        {
            string result;
            var ps = PluralizationService.CreateService(CultureInfo.CurrentCulture);
            if (ps.IsPlural(target))
            {
                result = count == Count.Plural ? target : ps.Singularize(target);
            }
            else
            {
                result = count == Count.Singular ? target : ps.Pluralize(target);
            }

            return result;
        }

        public static string ConvertDotNotationToSpaceDelimited(string dotDelimited)
        {
            return dotDelimited.Replace('.', ' ');
        }

        public static string SplitCamelCase(string camelCase, string separator = " ")
        {
            var result = CamelCaseWithAcronymsWordRegex.Replace(camelCase, separator + "$1$2");
            foreach (var kv in TitleCaseeLowerCaseWordsSpacedToLower)
            {
                result = result.Replace(kv.Key, kv.Value);
            }

            return result;
        }
    }
}
