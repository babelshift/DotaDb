using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly SteamWebInterfaceFactory steamWebInterfaceFactory;

        private readonly string minimapIconsBaseUrl;
        private readonly string inStoreItemIconsBaseUrl;
        private readonly string itemIconsBaseUrl;
        private readonly string cosmeticItemsStoreBaseUrl;
        private readonly string itemAbilitiesFileName;
        private readonly string cosmeticItemsFileName;

        private const string tooltipAbilityPrefix = "DOTA_Tooltip_Ability";

        public ItemService(
            IConfiguration configuration,
            ISchemaParser schemaParser,
            CacheService cacheService,
            LocalizationService localizationService,
            BlobStorageService blobStorageService,
            SharedService sharedService,
            HeroService heroService,
            IMapper mapper)
        {
            this.configuration = configuration;
            this.schemaParser = schemaParser;
            this.cacheService = cacheService;
            this.localizationService = localizationService;
            this.blobStorageService = blobStorageService;
            this.sharedService = sharedService;
            this.heroService = heroService;
            this.mapper = mapper;
            string steamWebApiKey = configuration["SteamWebApiKey"];
            steamWebInterfaceFactory = new SteamWebInterfaceFactory(steamWebApiKey);

            minimapIconsBaseUrl = configuration["ImageUrls:MinimapIconsBaseUrl"];
            inStoreItemIconsBaseUrl = configuration["ImageUrls:InStoreItemIconsBaseUrl"];
            itemIconsBaseUrl = configuration["ImageUrls:ItemIconsBaseUrl"];
            cosmeticItemsStoreBaseUrl = configuration["ImageUrls:CosmeticItemsStoreBaseUrl"];
            itemAbilitiesFileName = configuration["FileNames:ItemAbilities"];
            cosmeticItemsFileName = configuration["FileNames:CosmeticItems"];
        }

        public async Task<IReadOnlyCollection<GameItemDetailModel>> GetGameItemsAsync()
        {
            var itemsWithoutRecipes = (await GetGameItemsFromCacheAsync())
                .Where(x => !x.IsRecipe);

            var gameItemTasks = itemsWithoutRecipes.Select(async item => new GameItemDetailModel()
            {
                Cost = item.Cost,
                RawName = item.Name,
                Name = await localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{item.Name}"),
                Description = await localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{item.Name}_Description"),
                Lore = await localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{item.Name}_Lore"),
                Id = item.Id,
                IsRecipe = item.IsRecipe,
                SecretShop = item.IsAvailableAtSecretShop,
                SideShop = item.IsAvailableAtSideShop,
                IconPath = string.Format("{0}{1}_lg.png", itemIconsBaseUrl, item.IsRecipe ? "recipe" : item.Name.Replace("item_", ""))
            });
            var gameItems = await Task.WhenAll(gameItemTasks);

            var abilities = await GetItemAbilitiesAsync();

            var addAbilityTasks = gameItems.Select(gameItem =>
            {
                return AddAbilityDetailsAsync(gameItem, abilities);
            });
            await Task.WhenAll(addAbilityTasks);

            return gameItems;
        }

        private async Task AddAbilityDetailsAsync(GameItemDetailModel gameItem, IReadOnlyDictionary<uint, ItemAbility> abilities)
        {
            if (abilities.TryGetValue(gameItem.Id, out ItemAbility item))
            {
                string joinedBehaviors = sharedService.GetJoinedBehaviors(item.AbilityBehavior);
                string joinedUnitTargetTeamTypes = sharedService.GetJoinedUnitTargetTeamTypes(item.AbilityUnitTargetTeam);
                string joinedUnitTargetTypes = sharedService.GetJoinedUnitTargetTypes(item.AbilityUnitTargetType);
                string joinedUnitTargetFlags = sharedService.GetJoinedUnitTargetFlags(item.AbilityUnitTargetFlags);

                var abilitySpecials = item.AbilitySpecials
                    .Select(abilitySpecial => new GameItemAbilitySpecialDetailModel()
                    {
                        RawName = abilitySpecial.Name,
                        Value = abilitySpecial.Value,
                        LinkedSpecialBonus = abilitySpecial.LinkedSpecialBonus
                    })
                    .ToList()
                    .AsReadOnly();

                var note0Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{gameItem.RawName}_Note0");
                var note1Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{gameItem.RawName}_Note1");
                var note2Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{gameItem.RawName}_Note2");
                var note3Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{gameItem.RawName}_Note3");
                var note4Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{gameItem.RawName}_Note4");
                var note5Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipAbilityPrefix}_{gameItem.RawName}_Note5");

                gameItem.CastPoint = item.AbilityCastPoint.ToSlashSeparatedString();
                gameItem.CastRange = item.AbilityCastRange.ToSlashSeparatedString();
                gameItem.Cooldown = item.AbilityCooldown.ToSlashSeparatedString();
                gameItem.Damage = item.AbilityDamage.ToSlashSeparatedString();
                gameItem.Duration = item.AbilityDuration.ToSlashSeparatedString();
                gameItem.ManaCost = item.AbilityManaCost.ToSlashSeparatedString();
                gameItem.AbilitySpecials = abilitySpecials;
                gameItem.Behaviors = joinedBehaviors;
                gameItem.TargetFlags = joinedUnitTargetFlags;
                gameItem.TargetTypes = joinedUnitTargetTypes;
                gameItem.TeamTargets = joinedUnitTargetTeamTypes;
                gameItem.Note0 = await note0Task;
                gameItem.Note1 = await note1Task;
                gameItem.Note2 = await note2Task;
                gameItem.Note3 = await note3Task;
                gameItem.Note4 = await note4Task;
                gameItem.Note5 = await note5Task;
                gameItem.CastsOnPickup = item.ItemCastOnPickup;
                gameItem.ContributesToNetWorthWhenDropped = item.ItemContributesToNetWorthWhenDropped;
                gameItem.Declarations = sharedService.GetJoinedItemDeclarationTypes(item.ItemDeclarations);
                gameItem.DisassembleRule = sharedService.GetJoinedItemDisassembleTypes(item.ItemDisassembleRule);
                gameItem.DisplayCharges = item.ItemDisplayCharges;
                gameItem.InitialCharges = item.ItemInitialCharges;
                gameItem.IsAlertable = item.ItemAlertable;
                gameItem.IsDroppable = item.ItemDroppable;
                gameItem.IsKillable = item.ItemKillable;
                gameItem.IsPermanent = item.ItemPermanent;
                gameItem.IsPurchasable = item.ItemPurchasable;
                gameItem.IsSellable = item.ItemSellable;
                gameItem.IsStackable = item.ItemStackable;
                gameItem.IsSupport = item.ItemSupport;
                gameItem.Shareability = sharedService.GetJoinedItemShareabilityTypes(item.ItemShareability);
                gameItem.ShopTags = GetSplitAndRejoinedShopTags(item.ItemShopTags);
                gameItem.StockInitial = item.ItemStockInitial;
                gameItem.StockMax = item.ItemStockMax;
                gameItem.StockTime = item.ItemStockTime;
                gameItem.IsNeutralDrop = item.ItemIsNeutralDrop;

                if (!string.IsNullOrWhiteSpace(item.ItemQuality))
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    gameItem.Quality = textInfo.ToTitleCase(item.ItemQuality.ReplaceUnderscoresWithSpaces());
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

        private async Task<IReadOnlyCollection<GameItem>> GetGameItemsFromCacheAsync()
        {
            return await cacheService.GetOrSetAsync(MemoryCacheKey.GameItems, async () =>
            {
                var dota2Econ = steamWebInterfaceFactory.CreateSteamWebInterface<DOTA2Econ>(new HttpClient());
                var gameItems = await dota2Econ.GetGameItemsAsync();
                return gameItems.Data;
            }, TimeSpan.FromDays(1));
        }

        private async Task<IReadOnlyDictionary<uint, ItemAbility>> GetItemAbilitiesAsync()
        {
            string cacheKey = $"parsed_{itemAbilitiesFileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schema", itemAbilitiesFileName);
                var itemAbilities = schemaParser.GetDotaItemAbilities(vdf);
                return new ReadOnlyDictionary<uint, ItemAbility>(itemAbilities.ToDictionary(itemAbility => itemAbility.Id, x => x));
            }, TimeSpan.FromDays(1));
        }

        public async Task<InStoreViewModel> GetCosmeticItemsAsync(int? page)
        {
            InStoreViewModel viewModel = new InStoreViewModel();

            // Get full schema
            var schema = await GetSchemaAsync();

            // Get heroes without "base" and "dummy"
            var heroes = await heroService.GetHeroesAsync();
            var filteredHeroes = heroes
                .Where(x => x.Value.Name != "npc_dota_hero_base"
                    && x.Value.Name != "npc_dota_hero_target_dummy")
                .Select(async x => await heroService.GetHeroNameAsync(x.Value));
            viewModel.Heroes = await Task.WhenAll(filteredHeroes);

            // Collect Prefabs, Rarities, and Qualities and fix up the string casing
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            viewModel.Prefabs = schema.Prefabs.Select(x => new InStoreItemPrefabViewModel()
            {
                Id = x.Type,
                Name = textInfo.ToTitleCase(x.Type.ReplaceUnderscoresWithSpaces())
            }).ToList().AsReadOnly();

            viewModel.Rarities = schema.Rarities.Select(x => new InStoreItemRarityViewModel()
            {
                Name = textInfo.ToTitleCase(x.Name.ReplaceUnderscoresWithSpaces())
            }).ToList().AsReadOnly();

            viewModel.Qualities = schema.Qualities.Select(x => new InStoreItemQualityViewModel()
            {
                Name = textInfo.ToTitleCase(x.Name.ReplaceUnderscoresWithSpaces())
            }).ToList().AsReadOnly();

            List<InStoreItemViewModel> inStoreItems = new List<InStoreItemViewModel>();
            foreach (var item in schema.Items)
            {
                // if there's a name or description, remove the "#" character before
                string itemName = RemoveHashPrefix(item.ItemName);
                string itemDescription = RemoveHashPrefix(item.ItemDescription);

                // look up the rarity and quality details
                var rarity = schema.Rarities.FirstOrDefault(x => x.Name == item.ItemRarity);
                var rarityColor = schema.Colors.FirstOrDefault(x => x.Name == rarity?.Color);
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
                                HeroName = await heroService.GetHeroNameAsync(hero.Value),
                                MinimapIconPath = hero.Value.GetMinimapIconFilePath(minimapIconsBaseUrl)
                            });
                        }
                    }
                }

                var itemViewModel = new InStoreItemViewModel()
                {
                    DefIndex = item.DefIndex,
                    Prefab = item.Prefab.ReplaceUnderscoresWithSpaces(),
                    Name = await localizationService.GetCosmeticItemLocalizationTextAsync(itemName),
                    Description = await localizationService.GetCosmeticItemLocalizationTextAsync(itemDescription),
                    IconPath = item.GetIconPath(inStoreItemIconsBaseUrl),
                    StorePath = $"{cosmeticItemsStoreBaseUrl}/{item.DefIndex}",
                    CreationDate = item.CreationDate,
                    ExpirationDate = item.ExpirationDate,
                    PriceBucket = item.PriceInfo?.Bucket ?? string.Empty,
                    PriceCategoryTags = item.PriceInfo?.CategoryTags ?? string.Empty,
                    PriceClass = item.PriceInfo?.Class ?? string.Empty,
                    PriceDate = item.PriceInfo?.Date,
                    Price = item.PriceInfo?.Price,
                    Rarity = rarity?.Name ?? string.Empty,
                    RarityColor = rarityColor?.HexColor ?? string.Empty,
                    Quality = quality?.Name ?? string.Empty,
                    QualityColor = quality?.HexColor ?? string.Empty,
                    UsedBy = usedByHeroes?.AsReadOnly() ?? null,
                    BundledItems = item.BundledItems != null ? item.BundledItems.ToList().AsReadOnly() : null,
                    Slot = item.ItemSlot
                };

                inStoreItems.Add(itemViewModel);
            }

            viewModel.Items = inStoreItems;

            return viewModel;
        }

        private static string RemoveHashPrefix(string s)
        {
            return !string.IsNullOrWhiteSpace(s) && s.StartsWith("#")
                ? s.Remove(0, 1)
                : string.Empty;
        }

        private async Task<Schema> GetSchemaAsync()
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