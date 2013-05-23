namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using MvcBootstrap.Util;

    public class Choices<TViewModel> : IChoices<TViewModel>
        where TViewModel : IEntityViewModel
    {
        public Choices()
        {
            this.Selections = Enumerable.Empty<TViewModel>();
            this.Options = Enumerable.Empty<TViewModel>();
        }

        public IEnumerable<TViewModel> Selections { get; set; }

        public IEnumerable<TViewModel> Options { get; set; }

        public override string ToString()
        {
            var selectionStrings = this.Selections.Select(vm => vm.ToString());
            return string.Join(", ", selectionStrings);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Choices<TViewModel>);
        }

        public bool Equals(Choices<TViewModel> other)
        {
            return other != null && 
                this.Selections.SequenceEqual(other.Selections) &&
                this.Options.SequenceEqual(other.Options);
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CalculateHashCode(
                base.GetHashCode(),
                () => this.Options);
        }
    }
}
