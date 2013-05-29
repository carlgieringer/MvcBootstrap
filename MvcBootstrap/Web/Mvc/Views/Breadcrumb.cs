namespace MvcBootstrap.Web.Mvc.Views
{
    public class Breadcrumb
    {
        public readonly string Text;

        public readonly string Path;

        public Breadcrumb(string text, string path)
        {
            this.Text = text;
            this.Path = path;
        }
    }
}