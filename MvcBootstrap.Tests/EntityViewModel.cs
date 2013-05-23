namespace MvcBootstrap.Tests
{
    using MvcBootstrap.ViewModels;

    public class EntityViewModel : IEntityViewModel
    {
        public EntityViewModel()
        {
            this.Choice = new Choice<EntityViewModel>();
        }

        public int? Id { get; set; }

        public byte[] Timestamp { get; set; }

        public IEntityViewModel ConcurrentlyEdited { get; set; }

        public Choice<EntityViewModel> Choice { get; set; } 
    }
}