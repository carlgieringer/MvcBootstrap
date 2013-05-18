namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;

    public class SortByClass<TEntity>
    {
        private readonly IList<SortByClass<TEntity>> thenBys; 

        public SortByClass(Func<TEntity, object> sortBy, SortOrder sortOrder = SortOrder.Ascending)
        {
            this.SortBy = sortBy;
            this.SortOrder = sortOrder;
            this.thenBys = new List<SortByClass<TEntity>>();
        }

        internal Func<TEntity, object> SortBy { get; private set; }

        internal SortOrder SortOrder { get; private set; }

        internal IEnumerable<SortByClass<TEntity>> ThenBys
        {
            get { return this.thenBys; }
        }

        public SortByClass<TEntity> ThenBy(Func<TEntity, object> sortBy)
        {
            this.thenBys.Add(new SortByClass<TEntity>(sortBy));
            return this;
        }

        public SortByClass<TEntity> ThenByDescending(Func<TEntity, object> sortBy)
        {
            this.thenBys.Add(new SortByClass<TEntity>(sortBy, SortOrder.Descending));
            return this;
        }
    }
}