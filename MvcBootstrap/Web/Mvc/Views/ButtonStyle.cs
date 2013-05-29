namespace MvcBootstrap.Web.Mvc.Views
{
    public enum ButtonStyle
    {
        /// <summary>
        /// A gray button with neutral implications.
        /// </summary>
        Default,

        /// <summary>
        /// A strong blue color indicating that the button is the more common
        /// of the choices from other buttons
        /// </summary>
        Primary,

        /// <summary>
        /// A light-blue button associated with non-urgent information.
        /// </summary>
        Info,

        /// <summary>
        /// A green style indicating successful progress.
        /// </summary>
        Success,

        /// <summary>
        /// A yellow style suggesting caution
        /// </summary>
        Warning,

        /// <summary>
        /// A strong red style suggesting a significant and/or irreversible action
        /// </summary>
        Danger,

        /// <summary>
        /// A dark style
        /// </summary>
        Inverse,

        /// <summary>
        /// Appears as a textual hyperlink
        /// </summary>
        Link,

        /// <summary>
        /// Not a button
        /// </summary>
        None
    }
}
