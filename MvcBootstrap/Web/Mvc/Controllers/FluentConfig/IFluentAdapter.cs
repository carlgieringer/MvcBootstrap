namespace MvcBootstrap.Web.Mvc.Controllers.FluentConfig
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Hides <see cref="Object"/> methods from the editor making the interface more fluent
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IFluentAdapter
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
}