namespace MvcBootstrap.Web.Mvc.Controllers.Extensions
{
    using System.Web.Mvc;

    using MvcBootstrap.Extensions;

    public static class ControllerBaseExtensions
    {
        private const string ControllerRouteKey = "controller";

        private const string ActionRouteKey = "action";

        /// <summary>
        /// Gets the controller's name.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static string Name(this ControllerBase controller)
        {
            return controller.ControllerContext.RouteData.Values[ControllerRouteKey].ToString();
        }

        /// <summary>
        /// Gets the name of the action targeted by the route.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static string ActionName(this ControllerBase controller)
        {
            return controller.ControllerContext.RouteData.Values[ActionRouteKey].ToString();
        }

        /// <summary>
        /// Returns true if <paramref name="controller"/> is a <see cref="HomeControllerBase"/>, false otherwise.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool IsHomeController(this ControllerBase controller)
        {
            return controller.GetType().IsAssignableTo(typeof(HomeControllerBase));
        }

        /// <summary>
        /// Returns true if <paramref name="controller"/> is a <see cref="IBootstrapController{TViewModel}"/>
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool IsBootstrapController(this ControllerBase controller)
        {
            return controller.GetType().IsConstructedGenericTypeOfDefinition(typeof(IBootstrapController<>));
        }
    }
}
