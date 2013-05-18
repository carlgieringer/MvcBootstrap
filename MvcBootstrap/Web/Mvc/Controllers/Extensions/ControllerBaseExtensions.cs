using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcBootstrap.Web.Mvc.Controllers.Extensions
{
    using System.Web.Mvc;

    public static class ControllerBaseExtensions
    {
        public static string ActionName(this ControllerBase controller)
        {
            return controller.ControllerContext.RouteData.Values["action"].ToString();
        }
    }
}
