namespace MvcBootstrap.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public abstract class EntityViewModelBase : IEntityViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public virtual int? Id { get; set; }

        [ScaffoldColumn(false)]
        public IEntityViewModel ConcurrentlyEdited { get; set; }

        [HiddenInput(DisplayValue = false)]
        public virtual byte[] Timestamp { get; set; }
    }
}