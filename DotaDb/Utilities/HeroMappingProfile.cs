using AutoMapper;
using DotaDb.Models;
using Steam.Models.DOTA2;

namespace DotaDb.Utilities
{
    public class HeroMappingProfile : Profile
    {
        public HeroMappingProfile()
        {
            CreateMap<HeroDetailModel, HeroViewModel>().ConvertUsing(new HeroToHeroViewModelConverter());
        }
    }
}