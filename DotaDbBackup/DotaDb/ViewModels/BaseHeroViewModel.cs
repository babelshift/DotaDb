using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public abstract class BaseHeroViewModel
    {
        public uint Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AvatarImagePath { get; set; }
        public uint BaseStrength { get; set; }
        public uint BaseAgility { get; set; }
        public uint BaseIntelligence { get; set; }
        public uint BaseDamageMin { get; set; }
        public uint BaseDamageMax { get; set; }
        public uint BaseMoveSpeed { get; set; }
        public double BaseArmor { get; set; }
        public string Team { get; set; }
        public double AttackRange { get; set; }
        public double AttackRate { get; set; }
        public double TurnRate { get; set; }
        public string AttackType { get; set; }
        public double StrengthGain { get; set; }
        public double AgilityGain { get; set; }
        public double IntelligenceGain { get; set; }
        public IReadOnlyCollection<HeroRoleViewModel> Roles { get; set; }
        public DotaHeroPrimaryAttributeType PrimaryAttribute { get; set; }
        public string ActiveTab { get; set; }
        public string MinimapIconPath { get; set; }
        public double BaseHealth { get; set; }
        public double BaseMana { get; set; }
        public IReadOnlyDictionary<int, double> HealthLevels { get; set; }
        public IReadOnlyDictionary<int, double> ManaLevels { get; set; }
        public IReadOnlyDictionary<int, double> MinDamageLevels { get; set; }
        public IReadOnlyDictionary<int, double> MaxDamageLevels { get; set; }
        public IReadOnlyDictionary<int, double> ArmorLevels { get; set; }
        public IReadOnlyDictionary<int, double> StrengthLevels { get; set; }
        public IReadOnlyDictionary<int, double> AgilityLevels { get; set; }
        public IReadOnlyDictionary<int, double> IntelligenceLevels { get; set; }
        public HeroTalentChoiceViewModel TalentChoiceAtLevel10 { get; set; }
        public HeroTalentChoiceViewModel TalentChoiceAtLevel15 { get; set; }
        public HeroTalentChoiceViewModel TalentChoiceAtLevel20 { get; set; }
        public HeroTalentChoiceViewModel TalentChoiceAtLevel25 { get; set; }
    }
}