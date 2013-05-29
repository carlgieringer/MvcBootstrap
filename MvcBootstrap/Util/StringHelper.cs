namespace MvcBootstrap.Util
{
    using System.Collections.Generic;
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;
    using System.Linq;

    public enum Count
    {
        Singular,
        Plural
    }

    public static class StringHelper
    {
        public static string Pluralize<T>(string target, IEnumerable<T> enumerable)
        {
            return Pluralize(target, enumerable.Count() == 1 ? Count.Singular : Count.Plural);
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
    }
}
