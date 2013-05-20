namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a non-generic base type for <see cref="ChoiceCollection{TViewModel}"/> so that
    /// strongly-typed Razor views can access <see cref="ChoiceCollection{TViewModel}.Choices"/>.
    /// </summary>
    public interface IChoiceCollection : ICollection<IEntityViewModel>
    {
        IEnumerable<IEntityViewModel> Choices { get; set; }
    }
}
