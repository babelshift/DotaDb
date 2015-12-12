using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DotaDb.Utilities;

namespace DotaDb.Controllers
{
    public class MatchesController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Live(long id)
        {
            var liveLeagueGame = await db.GetLiveLeagueGameAsync(id);

            LiveLeagueGameDetailViewModel viewModel = new LiveLeagueGameDetailViewModel()
            {
                BestOf = liveLeagueGame.BestOf,
                DireKillCount = liveLeagueGame.DireKillCount,
                DireTeamLogo = liveLeagueGame.DireTeamLogo,
                DireTeamName = liveLeagueGame.DireTeamName,
                ElapsedTime = liveLeagueGame.ElapsedTime,
                GameNumber = liveLeagueGame.GameNumber,
                LeagueLogoPath = liveLeagueGame.LeagueLogoPath,
                LeagueName = liveLeagueGame.LeagueName,
                RadiantKillCount = liveLeagueGame.RadiantKillCount,
                RadiantTeamLogo = liveLeagueGame.RadiantTeamLogo,
                RadiantTeamName = liveLeagueGame.RadiantTeamName,
                SeriesStatus = liveLeagueGame.SeriesStatus,
                SpectatorCount = liveLeagueGame.SpectatorCount,
                DirePlayers = liveLeagueGame.Players
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
                            PositionXPercent = y.PositionX.GetPercentOfPositionValue(),
                            PositionYPercent = y.PositionY.GetPercentOfPositionValue(),
                            MinimapIconFilePath = y.GetMinimapIconFilePath()
                        })
                        .ToList()
                        .AsReadOnly(),
                RadiantPlayers = liveLeagueGame.Players
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
                            PositionXPercent = y.PositionX.GetPercentOfPositionValue(),
                            PositionYPercent = y.PositionY.GetPercentOfPositionValue(),
                            MinimapIconFilePath = y.GetMinimapIconFilePath()
                        })
                        .ToList()
                        .AsReadOnly()
            };

            return View(viewModel);
        }
    }
}