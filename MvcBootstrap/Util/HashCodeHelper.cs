namespace MvcBootstrap.Util
{
    using System;

    public static class HashCodeHelper
    {
        /// <summary>
        /// Simplifies correctly calculating hash codes based upon
        /// Jon Skeet's answer <a href="http://stackoverflow.com/a/263416">here</a>
        /// </summary>
        /// <param name="thunks">
        /// Thunks that return all the values upon which
        /// the hash code should depend.
        /// </param>
        /// <returns>
        /// A hash code based upon the inputs
        /// </returns>
        public static int CalculateHashCode(int? seed = null, params Func<object>[] thunks)
        {
            // Overflow is okay; just wrap around
            unchecked
            {
                int hash = seed ?? 5;
                foreach (var member in thunks)
                {
                    hash = (hash * 29) + member().GetHashCode();
                }

                return hash;
            }
        }

        public static int CalculateHashCode(params Func<object>[] thunks)
        {
            return CalculateHashCode(null, thunks);
        }
    }
}
