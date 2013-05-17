namespace MvcBootstrap.Extensions
{
    using System;

    public static class StringExtensions
    {
        public static string F(this string format, params object[] args)
        {
            return String.Format(format, args);
        }
    }
}
