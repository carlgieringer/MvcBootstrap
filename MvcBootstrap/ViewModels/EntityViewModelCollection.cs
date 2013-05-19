namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class EntityViewModelCollection : Collection<IEntityViewModel>
    {
        public EntityViewModelCollection()
        {
            this.Choices = Enumerable.Empty<IEntityViewModel>();
        }

        public EntityViewModelCollection(IList<IEntityViewModel> list)
            : base(list)
        {
            this.Choices = list.ToArray();
        }

        public IEnumerable<IEntityViewModel> Choices { get; set; }
    }
}
