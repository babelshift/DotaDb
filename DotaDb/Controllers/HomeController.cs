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

namespace DotaDb.Controllers
{
    public class HomeController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index()
        {
            HomeViewModel viewModel = new HomeViewModel();

            var playerCounts = await db.GetPlayerCountsAsync();
            var leagues = await db.GetLeaguesAsync();
            var gameItems = await db.GetGameItemsAsync();
            var schema = await db.GetSchemaAsync();
            var heroes = await db.GetHeroesAsync();
            var heroAbilities = await db.GetHeroAbilitiesAsync();

            viewModel.HeroCount = heroes.Count;
            viewModel.HeroAbilityCount = heroAbilities.Count;
            viewModel.ShopItemCount = schema.Items.Count;
            viewModel.LeagueCount = leagues.Count;
            viewModel.InGameItemCount = gameItems.Count;
            viewModel.LiveLeagueGameCount = await db.GetLiveLeagueGameCountAsync();
            viewModel.InGamePlayerCount = playerCounts.InGamePlayerCount;
            viewModel.DailyPeakPlayerCount = playerCounts.DailyPeakPlayerCount;
            viewModel.AllTimePeakPlayerCount = playerCounts.AllTimePeakPlayerCount;

            viewModel.LiveLeagueGames = await GetTopLiveLeagueGameAsync();

            await SetupRandomHero(viewModel, heroes);

            return View(viewModel);
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
            viewModel.Roles = GetRoles(hero.Role, hero.RoleLevels).AsReadOnly();
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

        private static List<HeroRoleViewModel> GetRoles(string roles, string roleLevels)
        {
            string[] rolesSplit = roles.Split(',');
            string[] roleLevelsSplit = roleLevels.Split(',');

            List<HeroRoleViewModel> roleViewModels = new List<HeroRoleViewModel>();
            for (int i = 0; i < rolesSplit.Length; i++)
            {
                roleViewModels.Add(new HeroRoleViewModel()
                {
                    Name = rolesSplit[i],
                    Level = roleLevelsSplit[i]
                });
            }

            return roleViewModels;
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