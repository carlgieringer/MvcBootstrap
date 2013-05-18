namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using System.Web.Mvc;

    public static class ModelMetadataExtensions
    {
        public static bool ShouldShow(this ModelMetadata metadata, ViewDataDictionary viewData)
        {
            return 
                metadata.ShowForDisplay && 
                metadata.ModelType != typeof(System.Data.EntityState) && 
                !viewData.TemplateInfo.Visited(metadata);
        }

        public static bool ShouldEdit(this ModelMetadata metadata)
        {
            return !metadata.IsReadOnly;
        }

        public static bool IsHidden(this ModelMetadata metadata)
        {
            return metadata.TemplateHint == "HiddenInput";
        }
    }
}
