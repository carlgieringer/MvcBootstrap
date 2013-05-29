namespace MvcBootstrap.ViewModels
{
    using System;

    public interface IEntityViewModel
    {
        int? Id { get; set; }

        byte[] Timestamp { get; set; }

        /// <summary>
        /// Null, unless submission of the view model resulted in
        /// a concurrent edit conflict.  In this case this is set equal
        /// to a view model mapped from the databases current values.
        /// </summary>
        /// <remarks>
        /// Allows views to display and compare the new database values 
        /// that conflicted with this view model's current values
        /// </remarks>
        IEntityViewModel ConcurrentlyEdited { get; set; }

        /// <summary>
        /// Null, unless submission of the view model resulted in 
        /// a validation error in which case this property is set to
        /// a view model mapped from an entity refreshed from the 
        /// database values.
        /// </summary>
        /// <remarks>
        /// Provides access to valid values when an edit is invalid, e.g.,
        /// allows breadcrumbs to display a correct original value rather
        /// than an invalid (empty?) edited value.
        /// </remarks>
        IEntityViewModel OriginalValues { get; set; }
    }
}
