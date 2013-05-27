namespace MvcBootstrap.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public interface IEntity
    {
        int Id { get; }

        byte[] Timestamp { get; }

        DateTime Created { get; set; }

        DateTime Modified { get; set; }
    }
}
