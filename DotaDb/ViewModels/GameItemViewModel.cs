using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class GameItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Lore { get; set; }
        public string IconPath { get; set; }
        public int Cost { get; set; }
        public bool SecretShop { get; set; }
        public bool SideShop { get; set; }
        public bool IsRecipe { get; set; }
        public string Note0 { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string Note3 { get; set; }
        public string Note4 { get; set; }
        public string Note5 { get; set; }
        public string Note6 { get; set; }

        public IReadOnlyCollection<GameItemAbilitySpecialViewModel> Attributes { get; set; }
        public string Behaviors { get; set; }
        public string TeamTargets { get; set; }
        public string TargetTypes { get; set; }
        public string TargetFlags { get; set; }
        public string CastRange { get; set; }
        public string CastPoint { get; set; }
        public string Cooldown { get; set; }
        public string Duration { get; set; }
        public string Damage { get; set; }
        public string ManaCost { get; set; }
    }
}