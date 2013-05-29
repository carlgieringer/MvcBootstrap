namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;

    /// <summary>
    /// Identifies the built-in Bootstrap controller actions
    /// </summary>
    [Flags]
    public enum BootstrapActions
    {
        List,
        Create,
        Read,
        Update
    }
}
