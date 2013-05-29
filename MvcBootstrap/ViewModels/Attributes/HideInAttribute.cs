namespace MvcBootstrap.ViewModels.Attributes
{
    using System;

    using MvcBootstrap.Web.Mvc.Controllers;

    /// <summary>
    /// Identifies members that should not appear in when serving built-in MvcBootstrap actions identified by <see cref="Actions"/>.
    /// </summary>
    /// <remarks>
    /// When applied to the same member, this attribute supersedes <see cref="ShowInAttribute"/>.  In future implementations, this
    /// attribute could be superseded by instances of <see cref="ShowInAttribute"/> applied to members of derived classes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class HideInAttribute : Attribute
    {
        public readonly BootstrapActions Actions;

        public HideInAttribute(BootstrapActions actions)
        {
            this.Actions = actions;
        }
    }
}
