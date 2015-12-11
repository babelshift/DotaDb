using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Newtonsoft.Json;
using DotaDb.Models;
using DotaDb.ViewModels;
using SourceSchemaParser.Dota2;
using System.Threading.Tasks;

namespace DotaDb.Controllers
{
    public class ItemsController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index()
        {
            var items = await db.GetGameItemsAsync();

            var tasks = items
                .Where(x => x.Recipe == 0)
                .Select(async (x) => new GameItemViewModel()
                {
                    Cost = x.Cost,
                    Name = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_Ability_{0}", x.Name)),
                    Description = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Description", x.Name)),
                    Lore = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Lore", x.Name)),
                    Id = x.Id,
                    IsRecipe = x.Recipe == 1 ? true : false,
                    SecretShop = x.SecretShop == 1 ? true : false,
                    SideShop = x.SideShop == 1 ? true : false,
                    IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", x.Recipe == 1 ? "recipe" : x.Name.Replace("item_", "")),
                });
            var itemsViewModel = await Task.WhenAll(tasks);

            var abilities = await db.GetItemAbilitiesAsync();

            foreach (var itemViewModel in itemsViewModel)
            {
                await AddAbilityToItemViewModelAsync(itemViewModel, abilities);
            }

            return View(itemsViewModel.ToList());
        }

        private async Task AddAbilityToItemViewModelAsync(GameItemViewModel viewModel, IReadOnlyDictionary<int, DotaItemAbilitySchemaItem> abilities)
        {
            DotaItemAbilitySchemaItem ability = null;
            bool abilityExists = abilities.TryGetValue(viewModel.Id, out ability);

            if (abilityExists)
            {
                string joinedBehaviors = db.GetJoinedBehaviors(ability.AbilityBehavior);
                string joinedUnitTargetTeamTypes = db.GetJoinedUnitTargetTeamTypes(ability.AbilityUnitTargetTeam);
                string joinedUnitTargetTypes = db.GetJoinedUnitTargetTypes(ability.AbilityUnitTargetType);
                string joinedUnitTargetFlags = db.GetJoinedUnitTargetFlags(ability.AbilityUnitTargetFlags);

                List<HeroAbilitySpecialViewModel> abilitySpecialViewModels = new List<HeroAbilitySpecialViewModel>();
                foreach (var abilitySpecial in ability.AbilitySpecials)
                {
                    abilitySpecialViewModels.Add(new HeroAbilitySpecialViewModel()
                    {
                        Name = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, abilitySpecial.Name)),
                        RawName = abilitySpecial.Name,
                        Value = abilitySpecial.Value.ToSlashSeparatedString()
                    });
                }

                viewModel.CastPoint = ability.AbilityCastPoint.ToSlashSeparatedString();
                viewModel.CastRange = ability.AbilityCastRange.ToSlashSeparatedString();
                viewModel.Cooldown = ability.AbilityCooldown.ToSlashSeparatedString();
                viewModel.Damage = ability.AbilityDamage.ToSlashSeparatedString();
                viewModel.Duration = ability.AbilityDuration.ToSlashSeparatedString();
                viewModel.ManaCost = ability.AbilityManaCost.ToSlashSeparatedString();
                viewModel.Attributes = abilitySpecialViewModels;
                viewModel.Behaviors = joinedBehaviors;
                viewModel.TargetFlags = joinedUnitTargetFlags;
                viewModel.TargetTypes = joinedUnitTargetTypes;
                viewModel.TeamTargets = joinedUnitTargetTeamTypes;
                viewModel.Note0 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note0"));
                viewModel.Note1 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note1"));
                viewModel.Note2 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note2"));
                viewModel.Note3 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note3"));
                viewModel.Note4 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note4"));
                viewModel.Note5 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note5"));
                viewModel.CastsOnPickup = ability.ItemCastOnPickup;
                viewModel.ContributesToNetWorthWhenDropped = ability.ItemContributesToNetWorthWhenDropped;
                viewModel.Declarations = db.GetJoinedItemDeclarationTypes(ability.ItemDeclarations);
                viewModel.DisassembleRule = db.GetJoinedItemDisassembleTypes(ability.ItemDisassembleRule);
                viewModel.DisplayCharges = ability.ItemDisplayCharges;
                viewModel.InitialCharges = ability.ItemInitialCharges;
                viewModel.IsAlertable = ability.ItemAlertable;
                viewModel.IsDroppable = ability.ItemDroppable;
                viewModel.IsKillable = ability.ItemKillable;
                viewModel.IsPermanent = ability.ItemPermanent;
                viewModel.IsPurchasable = ability.ItemPurchasable;
                viewModel.IsSellable = ability.ItemSellable;
                viewModel.IsStackable = ability.ItemStackable;
                viewModel.IsSupport = ability.ItemSupport;
                viewModel.Shareability = db.GetJoinedItemShareabilityTypes(ability.ItemShareability);
                viewModel.ShopTags = GetSplitAndRejoinedShopTags(ability.ItemShopTags);
                viewModel.StockInitial = ability.ItemStockInitial;
                viewModel.StockMax = ability.ItemStockMax;
                viewModel.StockTime = ability.ItemStockTime;
            }
        }

        private string GetSplitAndRejoinedShopTags(string shopTags)
        {
            if (!String.IsNullOrEmpty(shopTags))
            {
                string[] split = shopTags.Split(';');
                return String.Join(", ", split);
            }
            else
            {
                return String.Empty;
            }
        }
    }
}