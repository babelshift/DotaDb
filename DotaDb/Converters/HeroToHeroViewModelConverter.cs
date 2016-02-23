using AutoMapper;
using DotaDb.ViewModels;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace DotaDb.Converters
{
    public class HeroToHeroViewModelConverter : ITypeConverter<HeroDetailModel, HeroViewModel>
    {
        public HeroViewModel Convert(ResolutionContext context)
        {
            var source = context.SourceValue as HeroDetailModel;

            HeroViewModel viewModel = new HeroViewModel();

            viewModel.Abilities = AutoMapperConfiguration.Mapper.Map<IReadOnlyCollection<HeroAbilityDetailModel>, IReadOnlyCollection<HeroAbilityViewModel>>(source.Abilities);
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
            viewModel.Roles = AutoMapperConfiguration.Mapper.Map<IReadOnlyCollection<HeroRoleModel>, IReadOnlyCollection<HeroRoleViewModel>>(source.Roles);
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