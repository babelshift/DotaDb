using DotaDb.Models;
using DotaDb.ViewModels;
using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;

namespace DotaDb.Controllers
{
    public class ItemsController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index(string tab, string prefab, int? page)
        {
            if (tab == "ingame")
            {
                var gameItems = await GetGameItemsAsync();
                return View(gameItems);
            }
            else if (tab == "instore")
            {
                InStoreViewModel viewModel = await GetInStoreItemsAsync(prefab, page);

                return View("IndexInStore", viewModel);
            }
            else
            {
                var gameItems = await GetGameItemsAsync();
                return View(gameItems);
            }
        }

        private async Task<InStoreViewModel> GetInStoreItemsAsync(string prefab, int? page)
        {
            var schema = await db.GetSchemaAsync();

            InStoreViewModel viewModel = new InStoreViewModel();

            viewModel.Prefabs = schema.Prefabs.Select(x => new InStoreItemPrefabViewModel()
            {
                Id = x.Type,
                Name = x.Type.Replace('_', ' ')
            })
            .ToList()
            .AsReadOnly();

            List<InStoreItemViewModel> inStoreItems = new List<InStoreItemViewModel>();
            foreach (var item in schema.Items)
            {
                // if the user picked a prefab and the item doesn't fit that, skip it
                if (!String.IsNullOrEmpty(prefab) && item.Prefab != prefab)
                {
                    continue;
                }

                string name = !String.IsNullOrEmpty(item.NameLocalized) ? item.NameLocalized.Remove(0, 1) : String.Empty;
                string description = !String.IsNullOrEmpty(item.DescriptionLocalized) ? item.DescriptionLocalized.Remove(0, 1) : String.Empty;

                var rarity = schema.Rarities.FirstOrDefault(x => x.Name == item.ItemRarity);
                var rarityColor = rarity != null ? schema.Colors.FirstOrDefault(x => x.Name == rarity.Color) : null;

                var quality = schema.Qualities.FirstOrDefault(x => x.Name == item.ItemQuality);

                var itemViewModel = new InStoreItemViewModel()
                {
                    Name = await db.GetInStoreItemLocalizationTextAsync(name),
                    Description = await db.GetInStoreItemLocalizationTextAsync(description),
                    IconPath = String.Format("http://dotadb.azureedge.net/instoreitemicons/{0}.jpg", item.DefIndex),
                    StorePath = String.Format("http://www.dota2.com/store/itemdetails/{0}", item.DefIndex),
                    CreationDate = item.CreationDate,
                    ExpirationDate = item.ExpirationDate,
                    PriceBucket = item.PriceInfo != null ? item.PriceInfo.Bucket : String.Empty,
                    PriceCategoryTags = item.PriceInfo != null ? item.PriceInfo.CategoryTags : String.Empty,
                    PriceClass = item.PriceInfo != null ? item.PriceInfo.Class : String.Empty,
                    PriceDate = item.PriceInfo != null ? item.PriceInfo.Date : null,
                    Price = item.PriceInfo != null ? item.PriceInfo.Price : null,
                    Rarity = rarity != null ? rarity.Name : String.Empty,
                    RarityColor = rarityColor != null ? rarityColor.HexColor : String.Empty,
                    Quality = quality != null ? quality.Name : String.Empty,
                    QualityColor = quality != null ? quality.HexColor : String.Empty
                };

                inStoreItems.Add(itemViewModel);
            }

            viewModel.Items = inStoreItems.ToPagedList(page ?? 1, 25);
            viewModel.SelectedPrefab = prefab;

            return viewModel;
        }

        private async Task<IReadOnlyCollection<GameItemViewModel>> GetGameItemsAsync()
        {
            var items = await db.GetGameItemsAsync();

            var itemsWithoutRecipes = items.Where(x => x.Recipe == 0);

            List<GameItemViewModel> gameItems = new List<GameItemViewModel>();
            foreach (var item in itemsWithoutRecipes)
            {
                gameItems.Add(new GameItemViewModel()
                {
                    Cost = item.Cost,
                    Name = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_Ability_{0}", item.Name)),
                    Description = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Description", item.Name)),
                    Lore = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Lore", item.Name)),
                    Id = item.Id,
                    IsRecipe = item.Recipe == 1 ? true : false,
                    SecretShop = item.SecretShop == 1 ? true : false,
                    SideShop = item.SideShop == 1 ? true : false,
                    IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", item.Recipe == 1 ? "recipe" : item.Name.Replace("item_", "")),
                });
            }

            var abilities = await db.GetItemAbilitiesAsync();

            foreach (var itemViewModel in gameItems)
            {
                await AddAbilityToItemViewModelAsync(itemViewModel, abilities);
            }

            return gameItems.AsReadOnly();
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