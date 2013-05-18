namespace MvcBootstrap.Web.Mvc.Controllers.Extensions
{
    public enum FlashKind
    {
        /// <summary>
        /// Information that's not bad, and not related to some affirmative action on the user's part.
        /// </summary>
        Info,

        /// <summary>
        /// Positive confirmation of some user action
        /// </summary>
        Success,

        /// <summary>
        /// Something didn't fail, but the outcome requires user awareness
        /// </summary>
        Warning,

        /// <summary>
        /// Something failed.
        /// </summary>
        Error,
    }
}