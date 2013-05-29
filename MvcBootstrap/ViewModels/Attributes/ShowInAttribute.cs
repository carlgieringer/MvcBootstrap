namespace MvcBootstrap.ViewModels.Attributes
{
    using System;

    using MvcBootstrap.Web.Mvc.Controllers;

    /// <summary>
    /// Identifies members that should appear in when serving built-in MvcBootstrap actions identified by <see cref="Actions"/>.
    /// </summary>
    /// <remarks>
    /// By default, all members of a view model are shown.  Although not currently supported, this attribute could override
    /// instances of <see cref="HideInAttribute"/> applied to members of a view model's base classes.  When applied to the
    /// same member, <see cref="HideInAttribute"/> supersedes this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class ShowInAttribute : Attribute
    {
        public readonly BootstrapActions Actions;

        public ShowInAttribute(BootstrapActions actions)
        {
            this.Actions = actions;
        }
    }
}
