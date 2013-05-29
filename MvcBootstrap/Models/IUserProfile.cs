namespace MvcBootstrap.Models
{
    using System;

    public interface IUserProfile : IEntity
    {
        string Username { get; set; }

        DateTime LastLogin { get; set; }
    }
}