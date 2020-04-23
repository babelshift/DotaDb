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
            CreateMap<HeroRoleModel, HeroRoleViewModel>();
            CreateMap<HeroAbilityDetailModel, HeroAbilityViewModel>()
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Replace("\\n", "<br />")));
            CreateMap<HeroAbilitySpecialDetailModel, HeroAbilitySpecialViewModel>();
        }
    }
}