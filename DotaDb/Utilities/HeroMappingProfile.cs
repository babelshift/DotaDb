using AutoMapper;
using DotaDb.Models;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Linq;

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
                }))
                .ForMember(dest => dest.Behaviors, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitPipesIntoList(src.Behaviors, context);
                }))
                .ForMember(dest => dest.TargetFlags, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitPipesIntoList(src.TargetFlags, context);
                }))
                .ForMember(dest => dest.TargetTypes, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitPipesIntoList(src.TargetTypes, context);
                }))
                .ForMember(dest => dest.TeamTargets, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitPipesIntoList(src.TeamTargets, context);
                }))
                .ForMember(dest => dest.CastPoint, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.CastPoint);
                }))
                .ForMember(dest => dest.CastRange, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.CastRange);
                }))
                .ForMember(dest => dest.Cooldown, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.Cooldown);
                }))
                .ForMember(dest => dest.Damage, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.Damage);
                }))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.Duration);
                }))
                .ForMember(dest => dest.ManaCost, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.ManaCost);
                }));

            // TODO: move to its own profile
            CreateMap<HeroAbilitySpecialDetailModel, HeroAbilitySpecialViewModel>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.Value);
                }));

            CreateMap<GameItemDetailModel, GameItemViewModel>();
            CreateMap<GameItemAbilitySpecialDetailModel, GameItemAbilitySpecialViewModel>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom((src, dest, w, context) =>
                {
                    return SplitSpacesIntoList(src.Value);
                }));
        }

        private IReadOnlyList<string> SplitSpacesIntoList(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<string>();
            }

            return source
                .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        private IReadOnlyList<string> SplitPipesIntoList(string source, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<string>();
            }

            var lookup = context.Items["lookup"] as IReadOnlyDictionary<string, string>;
            var raw = source.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
            return raw
               .Select(x => GetKeyValue(x, lookup)?.ToString())
               .Where(x => x != null)
               .ToList();
        }

        private T GetKeyValue<T, K>(K key, IReadOnlyDictionary<K, T> dict)
        {
            if (key == null || string.IsNullOrWhiteSpace(key.ToString()) || dict == null)
            {
                return default(T);
            }

            return dict.TryGetValue(key, out T value) ? value : default(T);
        }
    }
}