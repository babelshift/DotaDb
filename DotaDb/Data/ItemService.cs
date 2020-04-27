using DotaDb.Models;
using DotaDb.Utilities;
using Microsoft.Extensions.Configuration;
using SourceSchemaParser;
using Steam.Models.DOTA2;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class ItemService
    {
        private readonly IConfiguration configuration;
        private readonly ISchemaParser schemaParser;
        private readonly CacheService cacheService;
        private readonly LocalizationService localizationService;
        private readonly BlobStorageService blobStorageService;
        private readonly SharedService sharedService;
        private readonly HeroService heroService;
        private readonly SteamWebInterfaceFactory steamWebInterfaceFactory;

        private readonly string minimapIconsBaseUrl;
        private readonly string inStoreItemIconsBaseUrl;
        private readonly string itemAbilitiesFileName;
        private readonly string cosmeticItemsFileName;

        public ItemService(
            IConfiguration configuration,
            ISchemaParser schemaParser,
            CacheService cacheService,
            LocalizationService localizationService,
            BlobStorageService blobStorageService,
            SharedService sharedService,
            HeroService heroService)
        {
            this.configuration = configuration;
            this.schemaParser = schemaParser;
            this.cacheService = cacheService;
            this.localizationService = localizationService;
            this.blobStorageService = blobStorageService;
            this.sharedService = sharedService;
            this.heroService = heroService;
            string steamWebApiKey = configuration["SteamWebApiKey"];
            steamWebInterfaceFactory = new SteamWebInterfaceFactory(steamWebApiKey);

            minimapIconsBaseUrl = configuration["ImageUrls:MinimapIconsBaseUrl"];
            inStoreItemIconsBaseUrl = configuration["ImageUrls:InStoreItemIconsBaseUrl"];
            itemAbilitiesFileName = configuration["FileNames:ItemAbilities"];
            cosmeticItemsFileName = configuration["FileNames:CosmeticItems"];
        }

        public async Task<IReadOnlyCollection<GameItemViewModel>> GetGameItemsAsync(string searchedItemName)
        {
            var items = await GetGameItemsAsync();

            var itemsWithoutRecipes = items.Where(x => !x.IsRecipe);

            List<GameItemViewModel> gameItems = new List<GameItemViewModel>();
            foreach (var item in itemsWithoutRecipes)
            {
                string localizedName = await localizationService.GetAbilityLocalizationTextAsync(String.Format("DOTA_Tooltip_Ability_{0}", item.Name));

                if (!string.IsNullOrWhiteSpace(searchedItemName))
                {
                    if (localizedName.ToLower().Contains(searchedItemName.ToLower()))
                    {
                        gameItems.Add(new GameItemViewModel()
                        {
                            Cost = item.Cost,
                            Name = localizedName,
                            Description = await localizationService.GetAbilityLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Description", item.Name)),
                            Lore = await localizationService.GetAbilityLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Lore", item.Name)),
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
                        Description = await localizationService.GetAbilityLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Description", item.Name)),
                        Lore = await localizationService.GetAbilityLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Lore", item.Name)),
                        Id = item.Id,
                        IsRecipe = item.IsRecipe,
                        SecretShop = item.IsAvailableAtSecretShop,
                        SideShop = item.IsAvailableAtSideShop,
                        IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", item.IsRecipe ? "recipe" : item.Name.Replace("item_", "")),
                    });
                }
            }

            var abilities = await GetItemAbilitiesAsync();

            foreach (var itemViewModel in gameItems)
            {
                await AddAbilityToItemViewModelAsync(itemViewModel, abilities);
            }

            return gameItems.AsReadOnly();
        }

        private async Task<IReadOnlyCollection<GameItemModel>> GetGameItemsAsync()
        {
            return await cacheService.GetOrSetAsync("gameItems", async () =>
            {
                var dota2Econ = steamWebInterfaceFactory.CreateSteamWebInterface<DOTA2Econ>(new HttpClient());
                var gameItems = await dota2Econ.GetGameItemsAsync();
                return gameItems.Data;
            }, TimeSpan.FromDays(1));
        }

        private async Task<IReadOnlyDictionary<uint, ItemAbilitySchemaItemModel>> GetItemAbilitiesAsync()
        {
            string cacheKey = $"parsed_{itemAbilitiesFileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schema", itemAbilitiesFileName);
                var itemAbilities = schemaParser.GetDotaItemAbilities(vdf);
                return new ReadOnlyDictionary<uint, ItemAbilitySchemaItemModel>(itemAbilities.ToDictionary(itemAbility => itemAbility.Id, x => x));
            }, TimeSpan.FromDays(1));
        }

        private async Task AddAbilityToItemViewModelAsync(GameItemViewModel viewModel, IReadOnlyDictionary<uint, ItemAbilitySchemaItemModel> abilities)
        {
            if (abilities.TryGetValue(viewModel.Id, out ItemAbilitySchemaItemModel item))
            {
                string joinedBehaviors = sharedService.GetJoinedBehaviors(item.AbilityBehavior);
                string joinedUnitTargetTeamTypes = sharedService.GetJoinedUnitTargetTeamTypes(item.AbilityUnitTargetTeam);
                string joinedUnitTargetTypes = sharedService.GetJoinedUnitTargetTypes(item.AbilityUnitTargetType);
                string joinedUnitTargetFlags = sharedService.GetJoinedUnitTargetFlags(item.AbilityUnitTargetFlags);

                List<HeroAbilitySpecialViewModel> abilitySpecialViewModels = new List<HeroAbilitySpecialViewModel>();
                foreach (var abilitySpecial in item.AbilitySpecials)
                {
                    abilitySpecialViewModels.Add(new HeroAbilitySpecialViewModel()
                    {
                        Name = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, abilitySpecial.Name)),
                        RawName = abilitySpecial.Name,
                        Value = abilitySpecial.Value.ToSlashSeparatedString()
                    });
                }

                viewModel.CastPoint = item.AbilityCastPoint.ToSlashSeparatedString();
                viewModel.CastRange = item.AbilityCastRange.ToSlashSeparatedString();
                viewModel.Cooldown = item.AbilityCooldown.ToSlashSeparatedString();
                viewModel.Damage = item.AbilityDamage.ToSlashSeparatedString();
                viewModel.Duration = item.AbilityDuration.ToSlashSeparatedString();
                viewModel.ManaCost = item.AbilityManaCost.ToSlashSeparatedString();
                viewModel.Attributes = abilitySpecialViewModels;
                viewModel.Behaviors = joinedBehaviors;
                viewModel.TargetFlags = joinedUnitTargetFlags;
                viewModel.TargetTypes = joinedUnitTargetTypes;
                viewModel.TeamTargets = joinedUnitTargetTeamTypes;
                viewModel.Note0 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note0"));
                viewModel.Note1 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note1"));
                viewModel.Note2 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note2"));
                viewModel.Note3 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note3"));
                viewModel.Note4 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note4"));
                viewModel.Note5 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note5"));
                viewModel.CastsOnPickup = item.ItemCastOnPickup;
                viewModel.ContributesToNetWorthWhenDropped = item.ItemContributesToNetWorthWhenDropped;
                viewModel.Declarations = sharedService.GetJoinedItemDeclarationTypes(item.ItemDeclarations);
                viewModel.DisassembleRule = sharedService.GetJoinedItemDisassembleTypes(item.ItemDisassembleRule);
                viewModel.DisplayCharges = item.ItemDisplayCharges;
                viewModel.InitialCharges = item.ItemInitialCharges;
                viewModel.IsAlertable = item.ItemAlertable;
                viewModel.IsDroppable = item.ItemDroppable;
                viewModel.IsKillable = item.ItemKillable;
                viewModel.IsPermanent = item.ItemPermanent;
                viewModel.IsPurchasable = item.ItemPurchasable;
                viewModel.IsSellable = item.ItemSellable;
                viewModel.IsStackable = item.ItemStackable;
                viewModel.IsSupport = item.ItemSupport;
                viewModel.Shareability = sharedService.GetJoinedItemShareabilityTypes(item.ItemShareability);
                viewModel.ShopTags = GetSplitAndRejoinedShopTags(item.ItemShopTags);
                viewModel.StockInitial = item.ItemStockInitial;
                viewModel.StockMax = item.ItemStockMax;
                viewModel.StockTime = item.ItemStockTime;

                if (!string.IsNullOrWhiteSpace(item.ItemQuality))
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    viewModel.Quality = textInfo.ToTitleCase(item.ItemQuality.Replace("_", " "));
                }
            }
        }

        private string GetSplitAndRejoinedShopTags(string shopTags)
        {
            if (!string.IsNullOrWhiteSpace(shopTags))
            {
                string[] split = shopTags.Split(';');
                return string.Join(", ", split);
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<InStoreViewModel> GetCosmeticItemsAsync(string prefab, int? page)
        {
            var schema = await GetSchemaAsync();

            InStoreViewModel viewModel = new InStoreViewModel();

            viewModel.Prefabs = schema.Prefabs.Select(x => new InStoreItemPrefabViewModel()
            {
                Id = x.Type,
                Name = x.Type.Replace('_', ' ')
            }).ToList().AsReadOnly();

            var heroes = await heroService.GetHeroesAsync();
            List<InStoreItemViewModel> inStoreItems = new List<InStoreItemViewModel>();
            foreach (var item in schema.Items)
            {
                // if the user picked a prefab and the item doesn't fit that, skip it
                if (!String.IsNullOrEmpty(prefab) && item.Prefab != prefab)
                {
                    continue;
                }

                // if there's a name or description, remove the "#" character before
                string name = !string.IsNullOrWhiteSpace(item.ItemName) && item.ItemName.StartsWith("#") 
                    ? item.ItemName.Remove(0, 1) 
                    : string.Empty;
                string description = !string.IsNullOrWhiteSpace(item.ItemDescription) && item.ItemDescription.StartsWith("#") 
                    ? item.ItemDescription.Remove(0, 1) 
                    : string.Empty;

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
                                HeroName = await localizationService.GetLocalizationTextAsync(hero.Value.Name),
                                MinimapIconPath = hero.Value.GetMinimapIconFilePath(minimapIconsBaseUrl)
                            });
                        }
                    }
                }

                var itemViewModel = new InStoreItemViewModel()
                {
                    Name = await localizationService.GetCosmeticItemLocalizationTextAsync(name),
                    Description = await localizationService.GetCosmeticItemLocalizationTextAsync(description),
                    IconPath = item.GetIconPath(inStoreItemIconsBaseUrl),
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

            viewModel.Items = inStoreItems;
            viewModel.SelectedPrefab = prefab;

            return viewModel;
        }

        private async Task<SchemaModel> GetSchemaAsync()
        {
            string cacheKey = $"parsed_{cosmeticItemsFileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", cosmeticItemsFileName);
                return schemaParser.GetDotaSchema(vdf);
            }, TimeSpan.FromDays(1));
        }
    }
}