namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using MvcBootstrap.Extensions;
    using MvcBootstrap.Util;

    public class ChoiceCollection<TViewModel> : Collection<TViewModel>, IChoiceCollection
        where TViewModel : IEntityViewModel
    {
        public ChoiceCollection()
        {
            this.Choices = Enumerable.Empty<IEntityViewModel>();
        }

        public IEnumerable<IEntityViewModel> Choices { get; set; }

        #region ICollection<>/IEnumerable<> Implicit Implementations

        /*
         * Since IENtityViewModelCollection implements its interfaces on IEntityViewModel instead of
         * TViewModel, we must provide implicit implementations for those interfaces.
         */

        bool ICollection<IEntityViewModel>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        IEnumerator<IEntityViewModel> IEnumerable<IEntityViewModel>.GetEnumerator()
        {
            var enumerator = this.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        void ICollection<IEntityViewModel>.Add(IEntityViewModel item)
        {
            this.Add((TViewModel)item);
        }

        bool ICollection<IEntityViewModel>.Contains(IEntityViewModel item)
        {
            return this.Contains((TViewModel)item);
        }

        void ICollection<IEntityViewModel>.CopyTo(IEntityViewModel[] array, int arrayIndex)
        {
            
            this.CopyTo(array.Cast<TViewModel>().ToArray(), arrayIndex);
        }

        bool ICollection<IEntityViewModel>.Remove(IEntityViewModel item)
        {
            return this.Remove((TViewModel)item);
        }

        #endregion

        public override string ToString()
        {
            var memberStrings = this.Select<TViewModel, string>(vm => vm.ToString());
            return string.Format("({0})".F(string.Join(", ", memberStrings)));
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ChoiceCollection<TViewModel>);
        }

        public bool Equals(ChoiceCollection<TViewModel> other)
        {
            return other != null && 
                this.SequenceEqual<TViewModel>(other) &&
                this.Choices.SequenceEqual(other.Choices);
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CalculateHashCode(
                base.GetHashCode(),
                () => this.Choices);
        }
    }
}
