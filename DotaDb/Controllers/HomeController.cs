using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DotaDb.Utilities;
using System.Collections.Generic;
using SourceSchemaParser.Dota2;
using SteamWebAPI2.Models.DOTA2;
using System.Net.Http;
using Steam.Models.DOTA2;

namespace DotaDb.Controllers
{
    public class HomeController : BaseController
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index()
        {
            HomeViewModel viewModel = new HomeViewModel();

            var playerCounts = await db.GetPlayerCountsAsync();

            IReadOnlyDictionary<int, LeagueModel> leagues = null;
            try
            {
                leagues = await db.GetLeaguesAsync();
            }
            catch (HttpRequestException) { } // maybe log this in the future, for now do nothing
            viewModel.LeagueCount = leagues != null ? (int?)leagues.Count : null;

            var gameItems = await db.GetGameItemsAsync();
            var schema = await db.GetSchemaAsync();
            var heroes = await db.GetHeroesAsync();
            var heroAbilities = await db.GetHeroAbilitiesAsync();

            viewModel.HeroCount = heroes.Count;
            viewModel.HeroAbilityCount = heroAbilities.Count;
            viewModel.ShopItemCount = schema.Items.Count;
            viewModel.InGameItemCount = gameItems.Count;

            int? liveLeagueGameCount = null;
            try
            {
                liveLeagueGameCount = await db.GetLiveLeagueGameCountAsync();
            }
            catch (HttpRequestException) { } // maybe log this in the future, for now do nothing
            viewModel.LiveLeagueGameCount = liveLeagueGameCount;

            viewModel.InGamePlayerCount = playerCounts.InGamePlayerCount;
            viewModel.DailyPeakPlayerCount = playerCounts.DailyPeakPlayerCount;
            viewModel.AllTimePeakPlayerCount = playerCounts.AllTimePeakPlayerCount;

            IReadOnlyCollection<LiveLeagueGameOverviewViewModel> liveLeagueGames = null;

            try
            {
                liveLeagueGames = await GetTopLiveLeagueGameAsync();
            }
            catch (HttpRequestException ex) { } // maybe log this in the future, for now do nothing
            viewModel.LiveLeagueGames = liveLeagueGames;

            await SetupRandomHero(viewModel, heroes);
            await SetupRandomItems(viewModel, gameItems);

            return View(viewModel);
        }

        private async Task SetupRandomItems(HomeViewModel viewModel, IReadOnlyCollection<GameItemModel> gameItems)
        {
            var randomItems = gameItems
                .Where(x => !String.IsNullOrEmpty(x.Name.Trim())
                && x.Name.Trim() != "Undefined"
                && !x.IsRecipe)
                .ToList();

            var abilities = await db.GetItemAbilitiesAsync();

            Random r = new Random();
            GameItemViewModel itemViewModel1 = await GetRandomItem(r, randomItems, abilities);
            GameItemViewModel itemViewModel2 = await GetRandomItem(r, randomItems, abilities);
            GameItemViewModel itemViewModel3 = await GetRandomItem(r, randomItems, abilities);

            List<GameItemViewModel> randomGameItems = new List<GameItemViewModel>();
            randomGameItems.Add(itemViewModel1);
            randomGameItems.Add(itemViewModel2);
            randomGameItems.Add(itemViewModel3);

            viewModel.RandomGameItems = randomGameItems.AsReadOnly();
        }

        private async Task<GameItemViewModel> GetRandomItem(Random r, IList<GameItemModel> randomItems, IReadOnlyDictionary<int, DotaItemAbilitySchemaItem> abilities)
        {
            int index = r.Next(0, randomItems.Count);
            var randomItem1 = randomItems[index];
            GameItemViewModel itemViewModel = await GetItemViewModel(randomItem1);
            await AddAbilityToItemViewModelAsync(itemViewModel, abilities);
            randomItems.RemoveAt(index);
            return itemViewModel;
        }

        private async Task<GameItemViewModel> GetItemViewModel(GameItemModel randomItem1)
        {
            return new GameItemViewModel()
            {
                Cost = randomItem1.Cost,
                Name = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_Ability_{0}", randomItem1.Name)),
                Description = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Description", randomItem1.Name)),
                Lore = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_ability_{0}_Lore", randomItem1.Name)),
                Id = randomItem1.Id,
                IsRecipe = randomItem1.IsRecipe,
                SecretShop = randomItem1.IsAvailableAtSecretShop,
                SideShop = randomItem1.IsAvailableAtSideShop,
                IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", randomItem1.IsRecipe ? "recipe" : randomItem1.Name.Replace("item_", "")),
            };
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

        private async Task SetupRandomHero(HomeViewModel viewModel, IReadOnlyDictionary<int, DotaHeroSchemaItem> heroes)
        {
            viewModel.RandomHero = new HeroViewModel();

            Random r = new Random();
            int index = r.Next(0, heroes.Count);
            var randomHero = heroes.ElementAt(index).Value;

            await SetupHeroViewModelAsync(randomHero, viewModel.RandomHero);
            await SetupAbilitiesAsync(randomHero, viewModel.RandomHero);
        }
        
        private async Task<BaseHeroViewModel> SetupHeroViewModelAsync<T>(DotaHeroSchemaItem hero, T viewModel)
            where T : BaseHeroViewModel
        {
            viewModel.Id = hero.HeroId;
            viewModel.Url = hero.Url.ToLower();
            viewModel.Name = await db.GetLocalizationTextAsync(hero.Name);
            viewModel.Description = "<from localization -> npc_dota_hero_<heroname>_hype>";
            viewModel.AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_full.png", hero.Name.Replace("npc_dota_hero_", ""));
            viewModel.BaseAgility = hero.AttributeBaseAgility;
            viewModel.BaseArmor = hero.ArmorPhysical;
            viewModel.BaseDamageMax = hero.AttackDamageMax;
            viewModel.BaseDamageMin = hero.AttackDamageMin;
            viewModel.BaseMoveSpeed = hero.MovementSpeed;
            viewModel.BaseStrength = hero.AttributeBaseStrength;
            viewModel.BaseIntelligence = hero.AttributeBaseIntelligence;
            viewModel.AttackRate = hero.AttackRate;
            viewModel.AttackRange = hero.AttackRange;
            viewModel.Team = db.GetTeamTypeKeyValue(hero.Team).ToString();
            viewModel.TurnRate = hero.MovementTurnRate;
            viewModel.AttackType = db.GetAttackTypeKeyValue(hero.AttackCapabilities).ToString();
            viewModel.Roles = hero.GetRoles();
            viewModel.AgilityGain = hero.AttributeAgilityGain;
            viewModel.IntelligenceGain = hero.AttributeIntelligenceGain;
            viewModel.StrengthGain = hero.AttributeStrengthGain;
            viewModel.PrimaryAttribute = db.GetHeroPrimaryAttributeTypeKeyValue(hero.AttributePrimary);
            viewModel.MinimapIconPath = hero.GetMinimapIconFilePath();
            return viewModel;
        }

        private async Task SetupAbilitiesAsync(DotaHeroSchemaItem hero, HeroViewModel viewModel)
        {
            var abilities = await db.GetHeroAbilitiesAsync();

            List<HeroAbilityViewModel> abilityViewModels = new List<HeroAbilityViewModel>();

            await AddAbilityToViewModelAsync(hero.Ability1, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability2, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability3, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability4, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability5, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability6, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability7, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability8, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability9, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability10, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability11, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability12, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability13, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability14, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability15, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability16, abilities, abilityViewModels);

            viewModel.Abilities = abilityViewModels.AsReadOnly();
        }

        private async Task AddAbilityToViewModelAsync(string abilityName, IReadOnlyCollection<DotaAbilitySchemaItem> abilities, List<HeroAbilityViewModel> abilityViewModels)
        {
            if (String.IsNullOrEmpty(abilityName))
            {
                return;
            }

            var ability = abilities.FirstOrDefault(x => x.Name == abilityName);

            string joinedBehaviors = db.GetJoinedBehaviors(ability.AbilityBehavior);
            string joinedUnitTargetTeamTypes = db.GetJoinedUnitTargetTeamTypes(ability.AbilityUnitTargetTeam);
            string joinedUnitTargetTypes = db.GetJoinedUnitTargetTypes(ability.AbilityUnitTargetType);
            string joinedUnitTargetFlags = db.GetJoinedUnitTargetFlags(ability.AbilityUnitTargetFlags);

            List<HeroAbilitySpecialViewModel> abilitySpecialViewModels = new List<HeroAbilitySpecialViewModel>();
            foreach (var abilitySpecial in ability.AbilitySpecials)
            {
                abilitySpecialViewModels.Add(new HeroAbilitySpecialViewModel()
                {
                    Name = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, abilitySpecial.Name)),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value.ToSlashSeparatedString()
                });
            }

            var abilityViewModel = new HeroAbilityViewModel()
            {
                Id = ability.Id,
                Name = await db.GetLocalizationTextAsync(String.Format("{0}_{1}", "DOTA_Tooltip_ability", abilityName)),
                AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/abilities/{0}_lg.png", ability.Name),
                Description = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Description")),
                CastPoint = ability.AbilityCastPoint.ToSlashSeparatedString(),
                CastRange = ability.AbilityCastRange.ToSlashSeparatedString(),
                Cooldown = ability.AbilityCooldown.ToSlashSeparatedString(),
                Damage = ability.AbilityDamage.ToSlashSeparatedString(),
                DamageType = db.GetDamageTypeKeyValue(ability.AbilityUnitDamageType),
                Duration = ability.AbilityDuration.ToSlashSeparatedString(),
                ManaCost = ability.AbilityManaCost.ToSlashSeparatedString(),
                SpellImmunityType = db.GetSpellImmunityTypeKeyValue(ability.SpellImmunityType),
                Attributes = abilitySpecialViewModels,
                Behaviors = joinedBehaviors,
                AbilityType = db.GetHeroAbilityTypeKeyValue(ability.AbilityType),
                TargetFlags = joinedUnitTargetFlags,
                TargetTypes = joinedUnitTargetTypes,
                TeamTargets = joinedUnitTargetTeamTypes,
                Note0 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note0")),
                Note1 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note1")),
                Note2 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note2")),
                Note3 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note3")),
                Note4 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note4")),
                Note5 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note5")),
            };

            abilityViewModels.Add(abilityViewModel);
        }

        private async Task<IReadOnlyCollection<LiveLeagueGameOverviewViewModel>> GetTopLiveLeagueGameAsync()
        {
            var liveLeagueGames = await db.GetLiveLeagueGamesAsync(1);

            var liveLeagueGameViewModels = liveLeagueGames
                .Select(x => new LiveLeagueGameOverviewViewModel()
                {
                    MatchId = x.MatchId,
                    BestOf = x.BestOf,
                    DireKillCount = x.DireKillCount,
                    DireTeamLogo = x.DireTeamLogo,
                    DireTeamName = x.DireTeamName,
                    ElapsedTime = x.ElapsedTimeDisplay,
                    GameNumber = x.GameNumber,
                    LeagueLogoPath = x.LeagueLogoPath,
                    LeagueName = x.LeagueName,
                    RadiantKillCount = x.RadiantKillCount,
                    RadiantTeamLogo = x.RadiantTeamLogo,
                    RadiantTeamName = x.RadiantTeamName,
                    RadiantSeriesWins = x.RadiantSeriesWins,
                    DireSeriesWins = x.DireSeriesWins,
                    SpectatorCount = x.SpectatorCount,
                    RadiantTowerStates = x.RadiantTowerStates,
                    DireTowerStates = x.DireTowerStates,
                    DirePlayers = x.Players
                        .Where(y => y.Team == 1)
                        .Select(y => new LiveLeagueGamePlayerViewModel()
                        {
                            HeroName = y.HeroName,
                            HeroAvatarFilePath = y.HeroAvatarImageFilePath,
                            PlayerName = y.Name,
                            DeathCount = y.DeathCount,
                            KillCount = y.KillCount,
                            AssistCount = y.AssistCount,
                            PositionX = y.PositionX,
                            PositionY = y.PositionY,
                            PositionXPercent = y.PositionX.GetPercentOfPositionValue(),
                            PositionYPercent = y.PositionY.GetPercentOfPositionValue(),
                            MinimapIconFilePath = y.GetMinimapIconFilePath()
                        })
                        .ToList()
                        .AsReadOnly(),
                    RadiantPlayers = x.Players
                        .Where(y => y.Team == 0)
                        .Select(y => new LiveLeagueGamePlayerViewModel()
                        {
                            HeroName = y.HeroName,
                            HeroAvatarFilePath = y.HeroAvatarImageFilePath,
                            PlayerName = y.Name,
                            DeathCount = y.DeathCount,
                            KillCount = y.KillCount,
                            AssistCount = y.AssistCount,
                            PositionX = y.PositionX,
                            PositionY = y.PositionY,
                            PositionXPercent = y.PositionX.GetPercentOfPositionValue(),
                            PositionYPercent = y.PositionY.GetPercentOfPositionValue(),
                            MinimapIconFilePath = y.GetMinimapIconFilePath()
                        })
                        .ToList()
                        .AsReadOnly()
                })
                .ToList()
                .AsReadOnly();

            return liveLeagueGameViewModels;
        }
    }
}