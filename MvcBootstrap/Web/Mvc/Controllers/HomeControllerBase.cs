namespace MvcBootstrap.Web.Mvc.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// A simple controller to serve a home page.
    /// </summary>
    public abstract class HomeControllerBase : Controller
    {
        public virtual ActionResult Index()
        {
            return this.View();
        }
    }
}
