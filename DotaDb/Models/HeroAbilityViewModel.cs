using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.Models
{
    public class HeroAbilityViewModel
    {
        public HeroAbilityViewModel()
        {
            Attributes = new List<HeroAbilitySpecialViewModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarImagePath { get; set; }
        public string Description { get; set; }
        public IReadOnlyCollection<HeroAbilitySpecialViewModel> Attributes { get; set; }
        public string Behaviors { get; set; }
        public string TeamTargets { get; set; }
        public string TargetTypes { get; set; }
        public string TargetFlags { get; set; }
        public DotaSpellImmunityType SpellImmunityType { get; set; }
        public DotaDamageType DamageType { get; set; }
        public string CastRange { get; set; }
        public string CastPoint { get; set; }
        public string Cooldown { get; set; }
        public string Duration { get; set; }
        public string Damage { get; set; }
        public string ManaCost { get; set; }
        public DotaHeroAbilityType AbilityType { get; set; }
        public IReadOnlyCollection<string> Notes { get; set; }
        public bool HasLinkedSpecialBonus { get; set; }
    }
}