namespace MvcBootstrap.Views.Extensions
{
    using MvcBootstrap.Controllers.Extensions;
    using MvcBootstrap.Exceptions;

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
