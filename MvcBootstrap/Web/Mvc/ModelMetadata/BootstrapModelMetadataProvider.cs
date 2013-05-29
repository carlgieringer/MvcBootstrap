// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BootstrapModelMetadataProvider.cs" company="Carl Gieringer">
//   Carl Gieringer 2013
// </copyright>
// <summary>
//   A model metadata provider that incorporates <see cref="ShowInAttribute" /> and <see cref="HideInAttribute" />
//   as a <see cref="BootstrapActionVisibility" /> in the <see cref="ModelMetadata.AdditionalValues" /> under the
//   key <see cref="ActionVisibilityKey" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcBootstrap.Web.Mvc.ModelMetadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using MvcBootstrap.ViewModels.Attributes;
    using MvcBootstrap.Web.Mvc.Controllers;

    /// <summary>
    /// A model metadata provider that incorporates <see cref="ShowInAttribute"/> and <see cref="HideInAttribute"/>
    /// as a <see cref="BootstrapActionVisibility"/> in the <see cref="ModelMetadata.AdditionalValues"/> under the
    /// key <see cref="ActionVisibilityKey"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="HideInAttribute"/> takes precedence over <see cref="ShowInAttribute"/> on the same property 
    /// because explicit hiding of information is considered more important than explicit showing.
    /// </remarks>
    public class BootstrapModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        /// <summary>
        /// The key for the <see cref="BootstrapActionVisibility"/> in <see cref="ModelMetadata.AdditionalValues"/>
        /// </summary>
        public const string ActionVisibilityKey = "MvcBootstrap.ActionVisibilty";

        /// <summary>
        /// Gets the metadata for the specified property, including whether to hide or show the 
        /// </summary>
        /// <param name="attributes">
        /// The property's attributes
        /// </param>
        /// <param name="containerType">
        /// The type of the property's containing object
        /// </param>
        /// <param name="modelAccessor">
        /// I don't know
        /// </param>
        /// <param name="modelType">
        /// The type of the property
        /// </param>
        /// <param name="propertyName">
        /// The name of the property within the containing object
        /// </param>
        /// <returns>
        /// A <see cref="ModelMetadata"/> containing information from <see cref="DataAnnotationsModelMetadataProvider"/>
        /// along with additional information for MvcBootstrap
        /// </returns>
        protected override ModelMetadata CreateMetadata(
            IEnumerable<Attribute> attributes,
            Type containerType,
            Func<object> modelAccessor,
            Type modelType,
            string propertyName)
        {
            var metadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);

            var actionVisibility = new BootstrapActionVisibility();

            var showInAttribute = attributes.OfType<ShowInAttribute>().SingleOrDefault();
            if (showInAttribute != null) 
            {
                foreach (BootstrapActions action in Enum.GetValues(typeof(BootstrapActions)))
                {
                    if (showInAttribute.Actions.HasFlag(action))
                    {
                        actionVisibility[action.ToString()] = true;
                    }
                }
            }

            // HideInAttribute overrides ShowInAttribute by coming later; 
            // this is intended as we consider explicit hiding more important than explicit showing
            var hideInAttribute = attributes.OfType<HideInAttribute>().SingleOrDefault();
            if (hideInAttribute != null)
            {
                foreach (BootstrapActions action in Enum.GetValues(typeof(BootstrapActions)))
                {
                    if (hideInAttribute.Actions.HasFlag(action))
                    {
                        actionVisibility[action.ToString()] = false;
                    }
                }
            }
            
            metadata.AdditionalValues.Add(ActionVisibilityKey, actionVisibility);

            return metadata;
        }
    }
}
