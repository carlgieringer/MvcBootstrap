namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;

    public class SortBase<TEntity>
    {
        public static SortByClass<TEntity> By(Func<TEntity, object> sortBy)
        {
            return new SortByClass<TEntity>(sortBy);
        }

        public static SortByClass<TEntity> ByDescending(Func<TEntity, object> sortBy)
        {
            return new SortByClass<TEntity>(sortBy, SortOrder.Descending);
        }
    }
}