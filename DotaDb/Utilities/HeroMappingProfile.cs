using AutoMapper;
using DotaDb.Models;
using Microsoft.Extensions.Azure;
using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.Utilities
{
    public class HeroMappingProfile : Profile
    {
        public HeroMappingProfile()
        {
            CreateMap<HeroDetailModel, HeroViewModel>().ConvertUsing(new HeroToHeroViewModelConverter());
            CreateMap<HeroRoleModel, HeroRoleViewModel>();
            CreateMap<HeroAbilityDetailModel, HeroAbilityViewModel>()
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Replace("\\n", "<br />")))
             .ForMember(dest => dest.Notes, opt => opt.MapFrom((src, dest) =>
             {
                 List<string> notes = new List<string>();
                 if (!string.IsNullOrWhiteSpace(src.Note0)) notes.Add(src.Note0);
                 if (!string.IsNullOrWhiteSpace(src.Note1)) notes.Add(src.Note1);
                 if (!string.IsNullOrWhiteSpace(src.Note2)) notes.Add(src.Note2);
                 if (!string.IsNullOrWhiteSpace(src.Note3)) notes.Add(src.Note3);
                 if (!string.IsNullOrWhiteSpace(src.Note4)) notes.Add(src.Note4);
                 if (!string.IsNullOrWhiteSpace(src.Note5)) notes.Add(src.Note5);
                 if (!string.IsNullOrWhiteSpace(src.Note6)) notes.Add(src.Note6);
                 return notes.AsReadOnly();
             }));
            CreateMap<HeroAbilitySpecialDetailModel, HeroAbilitySpecialViewModel>();
        }
    }
}