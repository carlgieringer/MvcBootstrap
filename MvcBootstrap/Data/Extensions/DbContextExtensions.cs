namespace MvcBootstrap.Data.Extensions
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;

    public static class DbContextExtensions
    {
        public static ObjectContext GetObjectContext(this DbContext context)
        {
            var adapter = (IObjectContextAdapter)context;
            return adapter.ObjectContext;
        }

        public static ObjectStateManager GetObjectStateManager(this DbContext context)
        {
            return context.GetObjectContext().ObjectStateManager;
        }
    }
}
