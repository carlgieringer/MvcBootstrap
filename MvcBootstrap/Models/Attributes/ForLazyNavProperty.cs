namespace MvcBootstrap.Models.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Denotes a property used as an ID of a required navigational property.  
    /// Should always be applied in conjunction with <see cref="RequiredEagerIdAttribute"/>
    /// </summary>
    /// <remarks>
    /// <see cref="RequiredEagerIdAttribute"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ForLazyNavPropertyAttribute : ForeignKeyAttribute
    {
        public ForLazyNavPropertyAttribute(string name)
            : base(name)
        {
        }
    }
}
