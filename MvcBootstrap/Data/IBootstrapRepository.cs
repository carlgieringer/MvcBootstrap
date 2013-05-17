namespace MvcBootstrap.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;

    public interface IBootstrapRepository<TEntity> where TEntity : class, IEntity
    {
        IQueryable<TEntity> Items { get; }

        TEntity Add(TEntity entity);

        /// <summary>
        /// Saves any changes in the repository's context.
        /// </summary>
        /// <exception cref="ConcurrentUpdateException">
        /// When changes in the context conflict with changes in another context.
        /// </exception>
        void SaveChanges();

        /// <summary>
        /// Updates the values of the entity allowing for optimistic concurrency exceptions.
        /// Sets <see cref="IEntity.Modified"/> to <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="entity">The entity to update; need not be attached to the context</param>
        /// <returns>The attached updated entity</returns>
        TEntity Update(TEntity entity);

        TEntity Create();

        TEntity GetById(int id);

        IEnumerable<TEntity> GetAll();

        TEntity GetOrCreate(Expression<Func<TEntity, bool>> predicatesLambda);

        void Delete(TEntity entity);
    }
}
