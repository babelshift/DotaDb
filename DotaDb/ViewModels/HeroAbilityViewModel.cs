using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class HeroAbilityViewModel
    {
        public string Name { get; set; }
        public string AvatarImagePath { get; set; }
        public string Description { get; set; }
        public IReadOnlyCollection<HeroAbilitySpecialViewModel> Attributes { get; set; }
        public string Behaviors { get; set; }
        public IReadOnlyCollection<string> TeamTargets { get; set; }
        public IReadOnlyCollection<string> TargetTypes { get; set; }
        public IReadOnlyCollection<string> TargetFlags { get; set; }
        public string SpellImmunityType { get; set; }
        public string DamageType { get; set; }
        public string CastRange { get; set; }
        public string CastPoint { get; set; }
        public string Cooldown { get; set; }
        public string Duration { get; set; }
        public string Damage { get; set; }
        public string ManaCost { get; set; }
    }
}