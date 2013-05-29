namespace MvcBootstrap.Web.Mvc.Controllers
{
    using MvcBootstrap.Exceptions;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;

    public class FlashMessage
    {
        public readonly string Message;

        public readonly FlashKind Kind;

        public FlashMessage(string message, FlashKind kind)
        {
            this.Message = message;
            this.Kind = kind;
        }

        public string CssClass()
        {
            string cssClass;
            switch (this.Kind)
            {
                case FlashKind.Info:
                    cssClass = "alert-info";
                    break;

                case FlashKind.Success:
                    cssClass = "alert-success";
                    break;

                case FlashKind.Warning:
                    cssClass = string.Empty;
                    break;

                case FlashKind.Error:
                    cssClass = "alert-error";
                    break;

                default:
                    throw new UnhandledEnumException<FlashKind>(this.Kind);
            }

            return cssClass;
        }
    }
}