namespace MvcBootstrap.ViewModels
{
    using System;

    public interface IEntityViewModel
    {
        int? Id { get; set; }

        byte[] Timestamp { get; set; }

        IEntityViewModel ConcurrentlyEdited { get; set; }
    }
}
