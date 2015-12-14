using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DotaDb.Utilities;

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
            var liveLeagueGames = await db.GetLiveLeagueGamesAsync(5);
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

            viewModel.LiveLeagueGames = liveLeagueGameViewModels;

            return View(viewModel);
        }
    }
}