using System.Collections.Generic;

namespace DotaDb.Models
{
    public class GameItemViewModel
    {
        public GameItemViewModel()
        {
            AbilitySpecials = new List<GameItemAbilitySpecialViewModel>();
        }

        public uint Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Lore { get; set; }
        public string IconPath { get; set; }
        public uint Cost { get; set; }
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

        public IReadOnlyCollection<GameItemAbilitySpecialViewModel> AbilitySpecials { get; set; }
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
        public double? ChannelTime { get; set; }
        public string SharedCooldown { get; set; }
        public string ShopTags { get; set; }
        public string Quality { get; set; }
        public bool? IsStackable { get; set; }
        public string Shareability { get; set; }
        public uint? InitialCharges { get; set; }
        public uint? DisplayCharges { get; set; }
        public bool? IsPermanent { get; set; }
        public uint? StockMax { get; set; }
        public uint? StockInitial { get; set; }
        public double? StockTime { get; set; }
        public string Declarations { get; set; }
        public bool? IsSupport { get; set; }
        public bool? IsAlertable { get; set; }
        public bool? ContributesToNetWorthWhenDropped { get; set; }
        public bool? IsKillable { get; set; }
        public bool? IsSellable { get; set; }
        public bool? CastsOnPickup { get; set; }
        public string DisassembleRule { get; set; }
        public string RecipeResult { get; set; }
        public bool? IsDroppable { get; set; }
        public bool? IsPurchasable { get; set; }
        public bool? IsNeutralDrop { get; set; }
        public bool ShowingDetails { get; set; }
    }
}