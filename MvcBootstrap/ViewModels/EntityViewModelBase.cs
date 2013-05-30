namespace MvcBootstrap.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public abstract class EntityViewModelBase : IEntityViewModel
    {
        [HiddenInput(DisplayValue = false), Editable(false)]
        public virtual int? Id { get; set; }

        [ScaffoldColumn(false)]
        public IEntityViewModel ConcurrentlyEdited { get; set; }

        [ScaffoldColumn(false)]
        public IEntityViewModel OriginalValues { get; set; }

        [HiddenInput(DisplayValue = false), Editable(false)]
        public virtual byte[] Timestamp { get; set; }
    }
}