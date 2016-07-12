using AutoMapper;
using DotaDb.Converters;
using DotaDb.ViewModels;
using Steam.Models.DOTA2;

namespace DotaDb
{
    internal static class AutoMapperConfiguration
    {
        private static MapperConfiguration config;
        private static IMapper mapper;

        public static IMapper Mapper { get { return mapper; } }

        public static void Initialize()
        {
            if (config == null)
            {
                config = new MapperConfiguration(x =>
                {
                    x.CreateMap<HeroDetailModel, HeroViewModel>().ConvertUsing(new HeroToHeroViewModelConverter());
                    x.CreateMap<HeroDetailModel, HeroItemBuildViewModel>().ConvertUsing(new HeroToHeroItemBuildViewModelConverter());
                    x.CreateMap<HeroRoleModel, HeroRoleViewModel>();
                    x.CreateMap<HeroAbilityDetailModel, HeroAbilityViewModel>();
                    x.CreateMap<HeroAbilitySpecialDetailModel, HeroAbilitySpecialViewModel>();
                    x.CreateMap<DotaBlogFeedItem, DotaBlogFeedItemViewModel>();
                });
            }

            if (mapper == null)
            {
                mapper = config.CreateMapper();
            }
        }

        public static void Reset()
        {
            config = null;
            mapper = null;
        }
    }
}