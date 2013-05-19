namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class EntityViewModelCollection : Collection<IEntityViewModel>, IEntityViewModelCollection
    {
        public IEnumerable<IEntityViewModel> Choices { get; set; }
    }
}
