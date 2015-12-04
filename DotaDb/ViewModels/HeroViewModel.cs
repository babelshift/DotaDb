using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class HeroViewModel
    {
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
        public IReadOnlyCollection<HeroAbilityViewModel> Abilities { get; set; }
        public DotaHeroPrimaryAttributeType PrimaryAttribute { get; set; }
    }
}