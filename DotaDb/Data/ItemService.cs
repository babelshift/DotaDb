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
        private readonly SteamWebInterfaceFactory steamWebInterfaceFactory;

        public ItemService(
            IConfiguration configuration,
            ISchemaParser schemaParser,
            CacheService cacheService,
            LocalizationService localizationService,
            BlobStorageService blobStorageService,
            SharedService sharedService)
        {
            this.configuration = configuration;
            this.schemaParser = schemaParser;
            this.cacheService = cacheService;
            this.localizationService = localizationService;
            this.blobStorageService = blobStorageService;
            this.sharedService = sharedService;
            string steamWebApiKey = configuration["SteamWebApiKey"];
            steamWebInterfaceFactory = new SteamWebInterfaceFactory(steamWebApiKey);
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
            string fileName = "item_abilities.vdf";
            string cacheKey = $"parsed_{fileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schema", fileName);
                var itemAbilities = schemaParser.GetDotaItemAbilities(vdf);
                return new ReadOnlyDictionary<uint, ItemAbilitySchemaItemModel>(itemAbilities.ToDictionary(itemAbility => itemAbility.Id, x => x));
            }, TimeSpan.FromDays(1));
        }

        private async Task AddAbilityToItemViewModelAsync(GameItemViewModel viewModel, IReadOnlyDictionary<uint, ItemAbilitySchemaItemModel> abilities)
        {
            if (abilities.TryGetValue(viewModel.Id, out ItemAbilitySchemaItemModel ability))
            {
                string joinedBehaviors = sharedService.GetJoinedBehaviors(ability.AbilityBehavior);
                string joinedUnitTargetTeamTypes = sharedService.GetJoinedUnitTargetTeamTypes(ability.AbilityUnitTargetTeam);
                string joinedUnitTargetTypes = sharedService.GetJoinedUnitTargetTypes(ability.AbilityUnitTargetType);
                string joinedUnitTargetFlags = sharedService.GetJoinedUnitTargetFlags(ability.AbilityUnitTargetFlags);

                List<HeroAbilitySpecialViewModel> abilitySpecialViewModels = new List<HeroAbilitySpecialViewModel>();
                foreach (var abilitySpecial in ability.AbilitySpecials)
                {
                    abilitySpecialViewModels.Add(new HeroAbilitySpecialViewModel()
                    {
                        Name = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, abilitySpecial.Name)),
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
                viewModel.Note0 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note0"));
                viewModel.Note1 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note1"));
                viewModel.Note2 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note2"));
                viewModel.Note3 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note3"));
                viewModel.Note4 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note4"));
                viewModel.Note5 = await localizationService.GetAbilityLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", viewModel.Name, "Note5"));
                viewModel.CastsOnPickup = ability.ItemCastOnPickup;
                viewModel.ContributesToNetWorthWhenDropped = ability.ItemContributesToNetWorthWhenDropped;
                viewModel.Declarations = sharedService.GetJoinedItemDeclarationTypes(ability.ItemDeclarations);
                viewModel.DisassembleRule = sharedService.GetJoinedItemDisassembleTypes(ability.ItemDisassembleRule);
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
                viewModel.Shareability = sharedService.GetJoinedItemShareabilityTypes(ability.ItemShareability);
                viewModel.ShopTags = GetSplitAndRejoinedShopTags(ability.ItemShopTags);
                viewModel.StockInitial = ability.ItemStockInitial;
                viewModel.StockMax = ability.ItemStockMax;
                viewModel.StockTime = ability.ItemStockTime;
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
    }
}