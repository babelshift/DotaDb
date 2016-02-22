using DotaDb.Data;
using DotaDb.Utilities;
using DotaDb.ViewModels;
using PagedList;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class ItemsController : BaseController
    {
        private InMemoryDb db = InMemoryDb.Instance;

        [OutputCache(CacheProfile = "Default")]
        public async Task<ActionResult> Sets(int? page)
        {
            var schema = await db.GetSchemaAsync();

            List<ItemSetViewModel> itemSets = new List<ItemSetViewModel>();
            foreach (var itemSet in schema.ItemSets)
            {
                var itemSetViewModel = new ItemSetViewModel()
                {
                    Name = await db.GetItemsLocalizationTextAsync(itemSet.LocalizedName.Remove(0, 1)),
                    Items = itemSet.Items.ToList().AsReadOnly()
                };

                itemSets.Add(itemSetViewModel);
            }

            return View(itemSets.AsReadOnly());
        }

        [OutputCache(CacheProfile = "Default")]
        public async Task<ActionResult> Autographs(int? page)
        {
            var schema = await db.GetSchemaAsync();

            var autographs = await GetAutographItemsAsync(schema);

            return View(autographs.AsReadOnly());
        }

        private async Task<List<ItemAutographViewModel>> GetAutographItemsAsync(SchemaModel schema)
        {
            List<ItemAutographViewModel> autographs = new List<ItemAutographViewModel>();
            foreach (var autograph in schema.ItemAutographs)
            {
                var autographViewModel = new ItemAutographViewModel()
                {
                    Name = autograph.Name,
                    Autograph = autograph.Autograph,
                    Language = autograph.Language,
                    WorkshopLink = autograph.WorkshopLink,
                    Modifier = !String.IsNullOrEmpty(autograph.Modifier) ? await db.GetLocalizationTextAsync(autograph.Modifier.Remove(0, 1)) : String.Empty,
                    IconPath = autograph.GetIconPath()
                };

                autographs.Add(autographViewModel);
            }

            return autographs;
        }

        public async Task<ActionResult> Cosmetics(string prefab, int? page)
        {
            var viewModel = await GetCosmeticItemsAsync(prefab, page);
            return View(viewModel);
        }

        [OutputCache(CacheProfile = "Default")]
        public async Task<ActionResult> Index(string itemName)
        {
            var gameItems = await GetGameItemsAsync(itemName);
            if (!String.IsNullOrEmpty(itemName))
            {
                ViewBag.SearchItemName = itemName;
            }

            return View(gameItems);
        }

        private async Task<InStoreViewModel> GetCosmeticItemsAsync(string prefab, int? page)
        {
            var schema = await db.GetSchemaAsync();

            InStoreViewModel viewModel = new InStoreViewModel();

            viewModel.Prefabs = schema.Prefabs.Select(x => new InStoreItemPrefabViewModel()
            {
                Id = x.Type,
                Name = x.Type.Replace('_', ' ')
            }).ToList().AsReadOnly();

            var heroes = await db.GetHeroesAsync();
            List<InStoreItemViewModel> inStoreItems = new List<InStoreItemViewModel>();
            foreach (var item in schema.Items)
            {
                // if the user picked a prefab and the item doesn't fit that, skip it
                if (!String.IsNullOrEmpty(prefab) && item.Prefab != prefab)
                {
                    continue;
                }

                // if there's a name or description, remove the "#" character before
                string name = !String.IsNullOrEmpty(item.NameLocalized) ? item.NameLocalized.Remove(0, 1) : String.Empty;
                string description = !String.IsNullOrEmpty(item.DescriptionLocalized) ? item.DescriptionLocalized.Remove(0, 1) : String.Empty;

                // look up the rarity and quality details
                var rarity = schema.Rarities.FirstOrDefault(x => x.Name == item.ItemRarity);
                var rarityColor = rarity != null ? schema.Colors.FirstOrDefault(x => x.Name == rarity.Color) : null;
                var quality = schema.Qualities.FirstOrDefault(x => x.Name == item.ItemQuality);

                // setup the heroes that are able to use this item
                List<InStoreItemUsedByHeroViewModel> usedByHeroes = new List<InStoreItemUsedByHeroViewModel>();
                if (item.UsedByHeroes != null)
                {
                    foreach (var heroName in item.UsedByHeroes)
                    {
                        var hero = heroes.FirstOrDefault(x => x.Value.Name == heroName);
                        if (hero.Value != null)
                        {
                            usedByHeroes.Add(new InStoreItemUsedByHeroViewModel()
                            {
                                HeroId = hero.Value.HeroId,
                                HeroName = await db.GetLocalizationTextAsync(hero.Value.Name),
                                MinimapIconPath = hero.Value.GetMinimapIconFilePath()
                            });
                        }
                    }
                }

                var itemViewModel = new InStoreItemViewModel()
                {
                    Name = await db.GetItemsLocalizationTextAsync(name),
                    Description = await db.GetItemsLocalizationTextAsync(description),
                    IconPath = item.GetIconPath(),
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
                    QualityColor = quality != null ? quality.HexColor : String.Empty,
                    UsedBy = usedByHeroes != null ? usedByHeroes.AsReadOnly() : null,
                    BundledItems = item.BundledItems != null ? item.BundledItems.ToList().AsReadOnly() : null
                };

                inStoreItems.Add(itemViewModel);
            }

            viewModel.Items = inStoreItems.ToPagedList(page ?? 1, 25);
            viewModel.SelectedPrefab = prefab;

            return viewModel;
        }

        private async Task<IReadOnlyCollection<GameItemViewModel>> GetGameItemsAsync(string itemName)
        {
            var items = await db.GetGameItemsAsync();

            var itemsWithoutRecipes = items.Where(x => !x.IsRecipe);

            List<GameItemViewModel> gameItems = new List<GameItemViewModel>();
            foreach (var item in itemsWithoutRecipes)
            {
                string localizedName = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_Ability_{0}", item.Name));

                if (!String.IsNullOrEmpty(itemName))
                {
                    if (localizedName.ToLower().Contains(itemName.ToLower()))
                    {
                        gameItems.Add(new GameItemViewModel()
                        {
                            Cost = item.Cost,
                            Name = localizedName,
                            Description = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Description", item.Name)),
                            Lore = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Lore", item.Name)),
                            Id = item.Id,
                            IsRecipe = item.IsRecipe,
                            SecretShop = item.IsAvailableAtSecretShop,
                            SideShop = item.IsAvailableAtSideShop,
                            IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", item.IsRecipe ? "recipe" : item.Name.Replace("item_", "")),
                        });
                    }
                }
                else
                {
                    gameItems.Add(new GameItemViewModel()
                    {
                        Cost = item.Cost,
                        Name = localizedName,
                        Description = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Description", item.Name)),
                        Lore = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Lore", item.Name)),
                        Id = item.Id,
                        IsRecipe = item.IsRecipe,
                        SecretShop = item.IsAvailableAtSecretShop,
                        SideShop = item.IsAvailableAtSideShop,
                        IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", item.IsRecipe ? "recipe" : item.Name.Replace("item_", "")),
                    });
                }
            }

            var abilities = await db.GetItemAbilitiesAsync();

            foreach (var itemViewModel in gameItems)
            {
                await AddAbilityToItemViewModelAsync(itemViewModel, abilities);
            }

            return gameItems.AsReadOnly();
        }

        private async Task AddAbilityToItemViewModelAsync(GameItemViewModel viewModel, IReadOnlyDictionary<int, ItemAbilitySchemaItemModel> abilities)
        {
            ItemAbilitySchemaItemModel ability = null;
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