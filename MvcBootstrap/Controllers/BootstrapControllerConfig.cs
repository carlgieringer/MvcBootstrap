namespace MvcBootstrap.Controllers
{
    using System;

    public class BootstrapControllerConfig<TEntity, TViewModel> : IViewModelLabelSelector<TViewModel>
    {
        public string ListViewName { get; set; }

        public string CreateViewName { get; set; }
        
        public string ReadViewName { get; set; }
        
        public string UpdateViewName { get; set; }

        public SortByClass<TEntity> Sort { get; set; }

        public Func<TEntity, string> EntityLabelSelector { get; set; }

        public Func<TViewModel, string> ViewModelLabelSelector { get; set; }
    }
}