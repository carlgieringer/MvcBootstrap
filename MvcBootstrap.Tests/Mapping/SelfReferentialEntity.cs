namespace MvcBootstrap.Tests.Mapping
{
    using System;

    using MvcBootstrap.Models;

    public class SelfReferentialEntity : IEntity
    {
        public SelfReferentialEntity Other { get; set; }

        public int Id { get; set; }

        public byte[] Timestamp { get; private set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }
    }
}