using AutoMapper;
using DotaDb.Models;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotaDb.Utilities
{
    public class HeroToHeroViewModelConverter : ITypeConverter<HeroDetail, HeroViewModel>
    {
        public HeroViewModel Convert(HeroDetail source, HeroViewModel destination, ResolutionContext context)
        {
            HeroViewModel viewModel = new HeroViewModel();

            var abilities = source.Abilities
                .Where(x => x.AbilityType != DotaHeroAbilityType.TALENTS)
                .ToList()
                .AsReadOnly();

            var talents = source.Abilities
                .Where(x => x.AbilityType == DotaHeroAbilityType.TALENTS)
                .Select(x => new HeroTalentViewModel() { Id = x.Id, Name = x.Name })
                .ToList().AsReadOnly();

            if (talents.Count >= 2)
            {
                viewModel.TalentChoiceAtLevel10 = new HeroTalentChoiceViewModel() { HeroTalentChoice1 = talents[0], HeroTalentChoice2 = talents[1] };
            }

            if (talents.Count >= 4)
            {
                viewModel.TalentChoiceAtLevel15 = new HeroTalentChoiceViewModel() { HeroTalentChoice1 = talents[2], HeroTalentChoice2 = talents[3] };
            }

            if (talents.Count >= 6)
            {
                viewModel.TalentChoiceAtLevel20 = new HeroTalentChoiceViewModel() { HeroTalentChoice1 = talents[4], HeroTalentChoice2 = talents[5] };
            }

            if (talents.Count >= 8)
            {
                viewModel.TalentChoiceAtLevel25 = new HeroTalentChoiceViewModel() { HeroTalentChoice1 = talents[6], HeroTalentChoice2 = talents[7] };
            }

            var lookup = context.Items["lookup"] as IReadOnlyDictionary<string, DotaHeroAbilityBehaviorType>;
            viewModel.Abilities = context.Mapper.Map<IReadOnlyCollection<HeroAbilityDetail>, IReadOnlyCollection<HeroAbilityViewModel>>(abilities, opts => opts.Items["lookup"] = lookup);
            viewModel.ActiveTab = source.ActiveTab;
            viewModel.AgilityGain = source.AgilityGain;
            viewModel.AttackRange = source.AttackRange;
            viewModel.AttackRate = source.AttackRate;
            viewModel.AttackType = source.AttackType;
            viewModel.AvatarImagePath = source.AvatarImagePath;
            viewModel.BaseAgility = source.BaseAgility;
            viewModel.BaseArmor = source.BaseArmor;
            viewModel.BaseDamageMax = source.BaseDamageMax;
            viewModel.BaseDamageMin = source.BaseDamageMin;
            viewModel.BaseIntelligence = source.BaseIntelligence;
            viewModel.BaseMoveSpeed = source.BaseMoveSpeed;
            viewModel.BaseStrength = source.BaseStrength;
            viewModel.Description = source.Description;
            viewModel.Id = source.Id;
            viewModel.IntelligenceGain = source.IntelligenceGain;
            viewModel.MinimapIconPath = source.MinimapIconPath;
            viewModel.Name = source.Name;
            viewModel.PrimaryAttribute = source.PrimaryAttribute;
            viewModel.Roles = context.Mapper.Map<IReadOnlyCollection<HeroRole>, IReadOnlyCollection<HeroRoleViewModel>>(source.Roles);
            viewModel.StrengthGain = source.StrengthGain;
            viewModel.Team = source.Team;
            viewModel.TurnRate = source.TurnRate;
            viewModel.Url = source.Url;
            viewModel.HealthLevels = CreateLevelDictionary(source.GetHealth);
            viewModel.ManaLevels = CreateLevelDictionary(source.GetMana);
            viewModel.MaxDamageLevels = CreateLevelDictionary(source.GetMaxDamage);
            viewModel.MinDamageLevels = CreateLevelDictionary(source.GetMinDamage);
            viewModel.StrengthLevels = CreateLevelDictionary(source.GetStrength);
            viewModel.AgilityLevels = CreateLevelDictionary(source.GetAgility);
            viewModel.IntelligenceLevels = CreateLevelDictionary(source.GetIntelligence);
            viewModel.ArmorLevels = CreateLevelDictionary(source.GetArmor);
            viewModel.BaseHealth = source.GetBaseHealth();
            viewModel.BaseMana = source.GetBaseMana();

            return viewModel;
        }

        private IReadOnlyDictionary<int, double> CreateLevelDictionary(Func<int, double> calc)
        {
            Dictionary<int, double> dict = new Dictionary<int, double>();
            for (int i = 0; i < 25; i++)
            {
                dict.Add(i, calc(i));
            }
            return new ReadOnlyDictionary<int, double>(dict);
        }
    }
}