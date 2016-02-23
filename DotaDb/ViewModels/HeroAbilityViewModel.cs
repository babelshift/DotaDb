﻿using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class HeroAbilityViewModel
    {
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
        public string Note0 { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string Note3 { get; set; }
        public string Note4 { get; set; }
        public string Note5 { get; set; }
    }
}