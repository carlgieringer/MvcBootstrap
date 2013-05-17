namespace MvcBootstrap.Controllers.Extensions
{
    using System.IO;
    using System.Web.Mvc;

    using MvcBootstrap.Properties;

    public static class ControllerExtensions
    {
        public const string FlashMessageKey = "FlashMessageKey";

        public const string FlashKindKey = "FlashKindKey";

        public static void Flash(this Controller controller, string message, FlashKind kind = FlashKind.Info, bool currentViewOnly = false)
        {
            // Set flash in both view data and temp data so that the flash is displayed in either the current or next view
            controller.ViewData[FlashMessageKey] = message;
            controller.ViewData[FlashKindKey] = kind;
            if (!currentViewOnly)
            {
                controller.TempData[FlashMessageKey] = message;
                controller.TempData[FlashKindKey] = kind;
            }
        }

        /// <summary>
        /// Renders a partial view to a string.
        /// </summary>
        public static string Render(this Controller controller, [AspMvcView] string viewName, object model, ViewDataDictionary viewData = null)
        {
            // http://stackoverflow.com/questions/5532345/mvc-render-to-string-in-the-controller-or-else-were

            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            }

            if (viewData == null)
            {
                viewData = controller.ViewData;
            }
            viewData.Model = model;

            var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);

            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, viewData, controller.TempData, sw);

                // copy retVal state items to the html helper 
                //foreach (var item in viewContext.Controller.ViewData.ModelState)
                //{
                //    if (!viewContext.ViewData.ModelState.Keys.Contains(item.Key))
                //        viewContext.ViewData.ModelState.Add(item);
                //}

                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}