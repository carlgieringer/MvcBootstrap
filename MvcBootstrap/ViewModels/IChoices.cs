namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a non-generic base type for <see cref="Choices{TViewModel}"/> so that
    /// strongly-typed Razor views can access <see cref="Choices{TViewModel}.Options"/>.
    /// </summary>
    public interface IChoices<out TChoice> where TChoice : IEntityViewModel
    {
        IEnumerable<TChoice> Selections { get; }

        IEnumerable<TChoice> Options { get; }
    }
}
