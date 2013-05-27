namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using MvcBootstrap.Models;

    public class RelationConfig<TEntity>
    {
        /// <summary>
        /// Gets or sets a lambda that, given an entity, returns an enumerable
        /// of all the related entities which are possible members of a navigation relation.
        /// </summary>
        public Func<TEntity, IEnumerable<IEntity>> OptionsSelector { get; set; }

        /// <summary>
        /// Gets or sets a lambda that, given a view model for a related entity,
        /// returns a string representation of the view model.
        /// </summary>
        public LambdaExpression LabelSelector { get; set; }

        public bool? CanChooseSelf { get; set; }
    }
}
