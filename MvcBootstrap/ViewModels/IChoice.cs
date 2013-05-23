namespace MvcBootstrap.ViewModels
{
    using System.Collections.Generic;

    public interface IChoice<out TChoice> where TChoice : IEntityViewModel
    {
        TChoice Selection { get; }

        IEnumerable<TChoice> Options { get; }
    }
}