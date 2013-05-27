namespace MvcBootstrap.Tests.Binding
{
    using MvcBootstrap.ViewModels;

    public class TestEntityViewModel : IEntityViewModel
    {
        public TestEntityViewModel()
        {
            this.Choice = new Choice<TestEntityViewModel>();
        }

        public int? Id { get; set; }

        public byte[] Timestamp { get; set; }

        public IEntityViewModel ConcurrentlyEdited { get; set; }

        public Choice<TestEntityViewModel> Choice { get; set; } 
    }
}