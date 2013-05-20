namespace MvcBootstrap.Reflection
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public static class StringHelper
    {
        private static readonly Regex CamelCaseWithAcronymsWordRegex = new Regex(@"(\B[A-Z][^A-Z]+)|\B(?<=[^A-Z]+)([A-Z]+)(?![^A-Z])");
        
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

        public static string ConvertDotNotationToSpaceDelimited(string dotDelimited)
        {
            return dotDelimited.Replace('.', ' ');
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
