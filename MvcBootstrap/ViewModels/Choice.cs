namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using MvcBootstrap.Util;

    public class Choice<TViewModel> : IChoice<TViewModel>
        where TViewModel : class, IEntityViewModel
    {
        public Choice()
        {
            this.Options = Enumerable.Empty<TViewModel>();
        }

        public TViewModel Selection { get; set; }

        public IEnumerable<TViewModel> Options { get; set; }

        public override string ToString()
        {
            return this.Selection != null ? 
                this.Selection.ToString() :
                string.Empty;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Choice<TViewModel>);
        }

        public bool Equals(Choice<TViewModel> other)
        {
            return other != null &&
                this.Selection.Equals(other.Selection) &&
                this.Options.SequenceEqual(other.Options);
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CalculateHashCode(
                () => this.Selection,
                () => this.Options);
        }
    }
}
