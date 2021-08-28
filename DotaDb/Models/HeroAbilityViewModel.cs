using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.Models
{
    public class HeroAbilityViewModel
    {
        public HeroAbilityViewModel()
        {
            AbilitySpecials = new List<HeroAbilitySpecialViewModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarImagePath { get; set; }
        public string Description { get; set; }
        public IReadOnlyCollection<HeroAbilitySpecialViewModel> AbilitySpecials { get; set; }
        public IReadOnlyList<string> Behaviors { get; set; }
        public IReadOnlyList<string> TeamTargets { get; set; }
        public IReadOnlyList<string> TargetTypes { get; set; }
        public IReadOnlyList<string> TargetFlags { get; set; }
        public DotaSpellImmunityType SpellImmunityType { get; set; }
        public DotaDamageType DamageType { get; set; }
        public IReadOnlyList<string> CastRange { get; set; }
        public IReadOnlyList<string> CastPoint { get; set; }
        public IReadOnlyList<string> Cooldown { get; set; }
        public IReadOnlyList<string> Duration { get; set; }
        public IReadOnlyList<string> Damage { get; set; }
        public IReadOnlyList<string> ManaCost { get; set; }
        public DotaHeroAbilityType AbilityType { get; set; }
        public IReadOnlyCollection<string> Notes { get; set; }
        public bool HasLinkedSpecialBonus { get; set; }
    }
}