namespace MvcBootstrap.Mapping
{
    using AutoMapper;

    using MvcBootstrap.Models;
    using MvcBootstrap.ViewModels;

    public static class MappingHelper
    {
        public static IMappingExpression<TEntity, TViewModel> CreateEntityToViewModelMap<TEntity, TViewModel>()
            where TEntity : IEntity
            where TViewModel : IEntityViewModel
        {

            return Mapper.CreateMap<TEntity, TViewModel>()
                .ForMember(vm => vm.ConcurrentlyEdited, o => o.Ignore())
                .ForMember(vm => vm.Id, o => o.ResolveUsing(e => e.Id == 0 ? (int?)null : e.Id));
        }

        public static IMappingExpression<TViewModel, TEntity> CreateViewModelToEntityMap<TViewModel, TEntity>()
            where TEntity : IEntity
            where TViewModel : IEntityViewModel
        {
            return Mapper.CreateMap<TViewModel, TEntity>()
                .ForMember(e => e.Created, o => o.Ignore())
                .ForMember(e => e.Modified, o => o.Ignore());
        }
    }
}
