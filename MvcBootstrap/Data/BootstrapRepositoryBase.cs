namespace MvcBootstrap.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Models;
    using MvcBootstrap.Reflection;

    public abstract class BootstrapRepositoryBase<TEntity> : IBootstrapRepository<TEntity>
        where TEntity : class, IEntity, new()
    {
        #region Constructors

        public BootstrapRepositoryBase(DbContext context)
        {
            this.Context = context;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the repository's entities as a queryable interface.
        /// </summary>
        protected IQueryable<TEntity> Items
        {
            get
            {
                return this.Context.Set<TEntity>();
            }
        }

        private DbContext Context { get; set; }

        #endregion


        #region IBootstrapRepository Methods

        public TEntity Add(TEntity entity)
        {
            entity.Created = DateTime.Now;
            entity.Modified = DateTime.Now;
            return this.Context.Set<TEntity>().Add(entity);
        }

        public void SaveChanges()
        {
            try
            {
                this.Context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Convert Optimistic Concurrency exception

                // What if related entities are updated...generally need to handle more than one probably.
                var entry = ex.Entries.Single();

                // The current values reflect the attempted save; reset them to what's now in the database.
                entry.CurrentValues.SetValues(entry.GetDatabaseValues());

                throw new ConcurrentUpdateException(entry.Entity);
            }
        }

        public TEntity Update(TEntity entity)
        {
            var attached = this.GetById(entity.Id);
            if (attached == null)
            {
                throw new MvcBootstrapDataException(
                    "{0} with Id = {1} does not exist.".F(typeof(TEntity).Description(), entity.Id));
            }

            var entry = this.Context.Entry(attached);

            // The Timestamp must be in the original values for optimistic concurrency checking to occur.
            // Otherwise the context knows that the Timestamp has been modified in the context
            entry.OriginalValues["Timestamp"] = entity.Timestamp;

            // Update the context entry to reflect the values passed in with the parameter
            entry.CurrentValues.SetValues(entity);

            attached.Modified = DateTime.Now;

            return attached;
        }

        public TEntity Create()
        {
            var entity = this.Context.Set<TEntity>().Create();
            
            entity.Created = DateTime.Now;
            entity.Modified = DateTime.Now;

            this.OnCreate(entity);
            
            return entity;
        }

        public TEntity CreateAndAdd()
        {
            var entity = this.Create();
            entity = this.Add(entity);
            return entity;
        }

        public TEntity GetById(int id)
        {
            return this.Items.SingleOrDefault(e => e.Id == id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.Items.ToArray();
        }

        /// <summary>
        /// Returns an entity satisfying <paramref name="predicatesLambda"/>, creating one with the required
        /// values set if none exists.  A created entity is added to the unit of work.
        /// </summary>
        /// <param name="predicatesLambda">
        /// A lambda of the form <code>entity => entity.Member1 == CONSTANT && entity.Member2 == CONSTANT</code>
        /// </param>
        public TEntity GetOrCreate(Expression<Func<TEntity, bool>> predicatesLambda)
        {
            var entity = this.Items.SingleOrDefault(predicatesLambda);

            if (entity == null)
            {
                entity = new TEntity();

                var remainingPredicates = predicatesLambda.Body;
                while (remainingPredicates is BinaryExpression && remainingPredicates.NodeType == ExpressionType.AndAlso)
                {
                    var andAlso = (BinaryExpression)remainingPredicates;
                    var currentPredicate = andAlso.Left;
                    this.updateObjectFromPredicate(entity, currentPredicate);
                    remainingPredicates = andAlso.Right;
                }

                // remainingPredicates should contain a single predicate because it is no longer an AndAlso binary expression.
                this.updateObjectFromPredicate(entity, remainingPredicates);

                this.Context.Set<TEntity>().Add(entity);
            }

            return entity;
        }

        public void Delete(TEntity entity)
        {
            this.Context.Set<TEntity>().Remove(entity);
        }

        public TEntity GetFromLocal(TEntity entity)
        {
            var local = this.Context.Set<TEntity>().Local.SingleOrDefault(e => e.Id == entity.Id);
            return local;
        }

        public TEntity Attach(TEntity entity)
        {
            var attached = this.Context.Set<TEntity>().Attach(entity);
            return attached;
        }

        #endregion


        #region Extension Points

        /// <summary>
        /// Allows implementers to respond to the creation of a new entity, e.g.
        /// setting additional properties to initial values.
        /// </summary>
        /// <remarks>
        /// Implementers should not add <paramref name="entity"/> to the context;
        /// instead use <see cref="CreateAndAdd"/>.
        /// </remarks>
        /// <param name="entity"></param>
        public virtual void OnCreate(TEntity entity)
        {
            // Base implementation does nothing.
        }

        #endregion


        #region Private Helper Methods

        private void updateObjectFromPredicate(object entity, Expression predicate)
        {
            switch (predicate.NodeType)
            {
                case ExpressionType.Equal:
                    var equal = (BinaryExpression)predicate;
                    var memberExpression = equal.Left as MemberExpression;
                    if (memberExpression == null)
                    {
                        throw new InvalidOperationException("The left side of a predicate must be a member expression");
                    }

                    object value;
                    switch (equal.Right.NodeType)
                    {
                        case ExpressionType.Constant:
                            var constantExpression = (ConstantExpression)equal.Right;
                            value = constantExpression.Value;
                            break;

                        case ExpressionType.MemberAccess:
                            // http://stackoverflow.com/a/2616959/39396
                            var memberAccesss = (MemberExpression)equal.Right;

                            // Compiler has generated a closure class for the member access
                            // First get the expression where the compiler is accessing the closure's enclosed member-owner
                            var closureAccess = (MemberExpression)memberAccesss.Expression;

                            // Then get the closure itself (not sure why it is a constant expression, but it is.  I guess the C# specification would explain why.)
                            var closure = (ConstantExpression)closureAccess.Expression;

                            // Applying the closureAccess to the closure yields the enclosed member-owner, which actually owns the value we're interested in.
                            var memberOwner = ((FieldInfo)closureAccess.Member).GetValue(closure.Value);

                            // Applying the memberAccess to the memberOwner yields the value.
                            value = ((PropertyInfo)memberAccesss.Member).GetValue(memberOwner, null);

                            // This technique has not been tested against multiple levels of member access, e.g., x => x.Name == Grandparent.Parent.Name.
                            break;

                        default:
                            var messageFormat = "{0} is not supported on the right side of a predicate.";
                            throw new InvalidOperationException(string.Format(messageFormat, equal.Right.NodeType));
                    }

                    var memberName = memberExpression.Member.Name;
                    ReflectionHelper.SetProperty(entity, memberName, value);
                    break;

                default:
                    var message = "Only equality predicates AndAlso'd together are supported "
                                  + "(e.g.: x => x.Name == other.Name && x.Value == other.Value).";
                    throw new InvalidOperationException(message);
            }
        }

        #endregion
    }
}