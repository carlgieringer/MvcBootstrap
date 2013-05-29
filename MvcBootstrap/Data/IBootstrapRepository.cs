namespace MvcBootstrap.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;

    public interface IBootstrapRepository<TEntity> where TEntity : class, IEntity
    {
        /// <summary>
        /// Adds <paramref name="entity"/> to the underlying context in the modified state.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a new instance of <see cref="TEntity"/> with <see cref="IEntity.Created"/> and
        /// <see cref="IEntity.Modified"/> set to <see cref="DateTime.Now"/>.  
        /// </summary>
        /// <remarks>
        /// The returned instance is not added or attached to the repository's context.
        /// </remarks>
        /// <returns>
        /// A new instance of <see cref="TEntity"/>
        /// </returns>
        TEntity Create();

        /// <summary>
        /// Creates a new instance of <see cref="TEntity"/> with <see cref="IEntity.Created"/> and
        /// <see cref="IEntity.Modified"/> set to <see cref="DateTime.Now"/>.  
        /// </summary>
        /// <remarks>
        /// The returned instance is added to the repository's context.
        /// </remarks>
        /// <returns>
        /// A new instance of <see cref="TEntity"/>
        /// </returns>
        TEntity CreateAndAdd();

        /// <summary>
        /// Gets the <see cref="TEntity"/> with <see cref="IEntityId"/> equal to <paramref name="id"/>
        /// </summary>
        /// <param name="id">
        /// The key of entity to try to retrieve.
        /// </param>
        /// <returns>
        /// The <see cref="TEntity"/> with <see cref="IEntity.Id"/> equal to <paramref name="id"/>, if one exists, and
        /// null otherwise.
        /// </returns>
        TEntity GetById(int id);

        /// <summary>
        /// Gets all of the repositories entities as an enumerable interface.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Returns the <see cref="TEntity"/> matching <paramref name="predicatesLambda"/> or creates one if one is not found.
        /// </summary>
        /// <param name="predicatesLambda">
        /// A predicate of the form: <code>e => e.Property1 == "SomeValue" && e.Property2 && ... && e.PropertyN == aVariable</code>
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If more than one entity matches <paramref name="predicatesLambda"/> or 
        /// if <paramref name="predicatesLambda"/> is in an invalid format.
        /// </exception>
        /// <returns>
        /// An entity matching <paramref name="predicatesLambda"/>
        /// </returns>
        TEntity GetOrCreate(Expression<Func<TEntity, bool>> predicatesLambda);

        /// <summary>
        /// Removes <paramref name="entity"/> from the context and marks it for deletion from the database
        /// </summary>
        /// <param name="entity"></param>
        void Delete(TEntity entity);

        /// <summary>
        /// Returns an entity if it is attached in the context, otherwise null.  It does not query the context.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        TEntity GetFromLocal(TEntity entity);

        /// <summary>
        /// Adds <paramref name="entity"/> to the underlying context in the unchanged state.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When <paramref name="entity"/> already exists in the underlying context.
        /// </exception>
        /// <param name="entity">
        /// A detached entity
        /// </param>
        /// <returns>
        /// The attached entity
        /// </returns>
        TEntity Attach(TEntity entity);

        /// <summary>
        /// Returns the properties of <paramref name="entity"/> to their original values
        /// and sets its status to unchanges.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        TEntity Reset(TEntity entity);
    }
}
