namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using System.Linq;
    using System.Web.Mvc;

    using MvcBootstrap.Web.Mvc.Controllers;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;
    using MvcBootstrap.Web.Mvc.ModelMetadata;

    public static class ModelMetadataExtensions
    {
        public static bool ShouldShow(this ModelMetadata metadata, ViewContext viewContext)
        {
            return 
                metadata.ShowForDisplay && 
                metadata.IsVisibleInAction(viewContext) &&
                metadata.ModelType != typeof(System.Data.EntityState) && 
                !viewContext.ViewData.TemplateInfo.Visited(metadata);
        }

        /// <summary>
        /// Determines whether this model should be shown in the current action.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static bool IsVisibleInAction(this ModelMetadata metadata, ViewContext viewContext)
        {
            var actionName = viewContext.Controller.ActionName();

            object actionVisibilityObject;
            if (metadata.AdditionalValues.TryGetValue(BootstrapModelMetadataProvider.ActionVisibilityKey, out actionVisibilityObject))
            {
                var actionVisibility = actionVisibilityObject as BootstrapActionVisibility;
                if (actionVisibility != null)
                {
                    bool visible;
                    if (actionVisibility.TryGetValue(actionName, out visible))
                    {
                        return visible;
                    }
                }
            }

            return true;
        }

        public static bool ShouldEdit(this ModelMetadata metadata)
        {
            return !metadata.IsReadOnly;
        }

        public static bool IsHiddenInput(this ModelMetadata metadata)
        {
            return metadata.TemplateHint == "HiddenInput";
        }
    }
}
