namespace MvcBootstrap.Models.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Denotes a property used as an ID of a required navigational property.  
    /// Should always be applied in conjunction with <see cref="ForLazyNavPropertyAttribute"/>
    /// </summary>
    /// <remarks>
    /// When a required navigational property is lazy-loaded (i.e., an Entity property that has
    /// the Required attribute applied and is declared with virtual keyword), Entity Framework
    /// validation will fail during DbContext.SaveChanges if the lazy navigation property has not
    /// been loaded during the request.  I.e., EntityFramework will not load required lazy 
    /// navigational properties, and instead will fail validation if these properties have not been loaded.
    /// 
    /// http://blogs.msdn.com/b/adonet/archive/2011/01/31/using-dbcontext-in-ef-feature-ctp5-part-6-loading-related-entities.aspx
    /// 
    /// To avoid this, we explicitly add the nav property's foreign key id to the model as a required nullable integer.
    /// Integers (as all scalar types) are eager loaded from the database since they exist in the model's table.
    /// Therefore, Required validation will not fail for them when they are loaded from the database.  Since the Id
    /// is nullable, it is still possible programmatically to have a missing navigation model, and this will
    /// fail validation. 
    /// 
    /// Note that the reason that both <see cref="RequiredEagerIdAttribute"/> and <see cref="ForLazyNavPropertyAttribute"/>
    /// must be used together is that we cannot make a single attribute inherit from <see cref="RequiredAttribute"/> and
    /// <see cref="ForeignKeyAttribute"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredEagerIdAttribute : RequiredAttribute
    {
    }
}