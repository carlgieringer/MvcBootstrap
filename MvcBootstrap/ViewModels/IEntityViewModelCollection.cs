namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;

    public interface IEntityViewModelCollection : ICollection<IEntityViewModel>
    {
        IEnumerable<IEntityViewModel> Choices { get; set; }
    }
}
