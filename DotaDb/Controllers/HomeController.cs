using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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

            viewModel.HeroCount = db.GetHeroes().Count;
            viewModel.HeroAbilityCount = db.GetHeroAbilities().Count;
            viewModel.ShopItemCount = db.GetSchema().Items.Count;
            viewModel.LeagueCount = leagues.Count;
            viewModel.InGameItemCount = db.GetGameItems().Count;
            viewModel.LiveLeagueGameCount = await db.GetLiveLeagueGameCountAsync();
            viewModel.InGamePlayerCount = playerCounts.InGamePlayerCount;
            viewModel.DailyPeakPlayerCount = playerCounts.DailyPeakPlayerCount;
            viewModel.AllTimePeakPlayerCount = playerCounts.AllTimePeakPlayerCount;

            var liveLeagueGameViewModels = liveLeagueGames
                .Select(x => new LiveLeagueGameOverviewViewModel()
                {
                    BestOf = x.BestOf,
                    DireKillCount = x.DireKillCount,
                    DireTeamLogo = x.DireTeamLogo,
                    DireTeamName = x.DireTeamName,
                    ElapsedTime = x.ElapsedTime,
                    GameNumber = x.GameNumber,
                    LeagueLogo = x.LeagueLogo,
                    LeagueName = x.LeagueName,
                    RadiantKillCount = x.RadiantKillCount,
                    RadiantTeamLogo = x.RadiantTeamLogo,
                    RadiantTeamName = x.RadiantTeamName,
                    SeriesStatus = x.SeriesStatus,
                    SpectatorCount = x.SpectatorCount,
                    DirePlayers = x.Players
                        .Where(y => y.Team == 1)
                        .Select(y => new LiveLeagueGamePlayerViewModel()
                        {
                            HeroName = y.HeroName,
                            HeroAvatarFileName = y.HeroAvatarImageFileName,
                            PlayerName = y.Name,
                            DeathCount = y.DeathCount,
                            KillCount = y.KillCount,
                            AssistCount = y.AssistCount,
                            PositionX = y.PositionX,
                            PositionY = y.PositionY,
                            PositionXPercent = ((y.PositionX + 7500) / 15000) * 100,
                            PositionYPercent = ((y.PositionY + 7500) / 15000) * 100,
                            MinimapIconFileName = !String.IsNullOrEmpty(y.HeroUrl) ? String.Format("{0}_icon.png", y.HeroUrl) : String.Empty
                        })
                        .ToList()
                        .AsReadOnly(),
                    RadiantPlayers = x.Players
                        .Where(y => y.Team == 0)
                        .Select(y => new LiveLeagueGamePlayerViewModel()
                        {
                            HeroName = y.HeroName,
                            HeroAvatarFileName = y.HeroAvatarImageFileName,
                            PlayerName = y.Name,
                            DeathCount = y.DeathCount,
                            KillCount = y.KillCount,
                            AssistCount = y.AssistCount,
                            PositionX = y.PositionX,
                            PositionY = y.PositionY,
                            PositionXPercent = ((y.PositionX + 7500) / 15000) * 100,
                            PositionYPercent = ((y.PositionY + 7500) / 15000) * 100,
                            MinimapIconFileName = !String.IsNullOrEmpty(y.HeroUrl) ? String.Format("{0}_icon.png", y.HeroUrl) : String.Empty
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