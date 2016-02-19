using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public abstract class BaseHeroViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AvatarImagePath { get; set; }
        public int BaseStrength { get; set; }
        public int BaseAgility { get; set; }
        public int BaseIntelligence { get; set; }
        public int BaseDamageMin { get; set; }
        public int BaseDamageMax { get; set; }
        public int BaseMoveSpeed { get; set; }
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
        public IList<double> HealthLevels { get; set; }
        public IList<double> ManaLevels { get; set; }
        public IList<double> MinDamageLevels { get; set; }
        public IList<double> MaxDamageLevels { get; set; }
        public IList<double> ArmorLevels { get; set; }
        public IList<double> StrengthLevels { get; set; }
        public IList<double> AgilityLevels { get; set; }
        public IList<double> IntelligenceLevels { get; set; }
    }
}