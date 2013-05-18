namespace MvcBootstrap.Web.Mvc.Views.Extensions
{
    using MvcBootstrap.Exceptions;
    using MvcBootstrap.Web.Mvc.Controllers.Extensions;

    public static class FlashKindExtensions
    {
        public static string ToAlertClass(this FlashKind kind)
        {
            switch (kind)
            {
                case FlashKind.Info:
                    return "alert-info";
                case FlashKind.Success:
                    return "alert-success";
                case FlashKind.Warning:
                    return string.Empty;
                case FlashKind.Error:
                    return "alert-error";
                default:
                    throw new UnhandledEnumException<FlashKind>(kind);
            }
        }
    }
}
