using DotaDb.Models;
using DotaDb.Utilities;
using DotaDb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class MatchesController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Live(long id)
        {
            var liveLeagueGame = await db.GetLiveLeagueGameAsync(id);
            if(liveLeagueGame == null)
            {
                return new HttpNotFoundResult();
            }

            LiveLeagueGameDetailViewModel viewModel = new LiveLeagueGameDetailViewModel()
            {
                BestOf = liveLeagueGame.BestOf,
                DireKillCount = liveLeagueGame.DireKillCount,
                DireTeamLogo = liveLeagueGame.DireTeamLogo,
                DireTeamName = liveLeagueGame.DireTeamName,
                ElapsedTime = liveLeagueGame.ElapsedTime,
                ElapsedTimeDisplay = liveLeagueGame.ElapsedTimeDisplay,
                GameNumber = liveLeagueGame.GameNumber,
                LeagueLogoPath = liveLeagueGame.LeagueLogoPath,
                LeagueName = liveLeagueGame.LeagueName,
                RadiantKillCount = liveLeagueGame.RadiantKillCount,
                RadiantTeamLogo = liveLeagueGame.RadiantTeamLogo,
                RadiantTeamName = liveLeagueGame.RadiantTeamName,
                RadiantSeriesWins = liveLeagueGame.RadiantSeriesWins,
                DireSeriesWins = liveLeagueGame.DireSeriesWins,
                SpectatorCount = liveLeagueGame.SpectatorCount,
                IsRoshanAlive = liveLeagueGame.RoshanRespawnTimer == 0 ? true : false,
                LobbyId = liveLeagueGame.LobbyId,
                MatchId = liveLeagueGame.MatchId,
                RadiantPickedHeroes = liveLeagueGame.RadiantPicks
                    .Select(x => new LiveLeagueGameHeroViewModel()
                    {
                        Id = x.Id,
                        AvatarImagePath = x.AvatarImagePath,
                        Name = x.Name,
                        Url = x.Url
                    })
                    .ToList()
                    .AsReadOnly(),
                RadiantBannedHeroes = liveLeagueGame.RadiantBans
                    .Select(x => new LiveLeagueGameHeroViewModel()
                    {
                        Id = x.Id,
                        AvatarImagePath = x.AvatarImagePath,
                        Name = x.Name,
                        Url = x.Url
                    })
                    .ToList()
                    .AsReadOnly(),
                DirePickedHeroes = liveLeagueGame.DirePicks
                    .Select(x => new LiveLeagueGameHeroViewModel()
                    {
                        Id = x.Id,
                        AvatarImagePath = x.AvatarImagePath,
                        Name = x.Name,
                        Url = x.Url
                    })
                    .ToList()
                    .AsReadOnly(),
                DireBannedHeroes = liveLeagueGame.DireBans
                    .Select(x => new LiveLeagueGameHeroViewModel()
                    {
                        Id = x.Id,
                        AvatarImagePath = x.AvatarImagePath,
                        Name = x.Name,
                        Url = x.Url
                    })
                    .ToList()
                    .AsReadOnly(),
                DirePlayers = liveLeagueGame.Players
                        .Where(y => y.Team == 1)
                        .Select(y => new LiveLeagueGamePlayerViewModel()
                        {
                            HeroId = y.HeroId,
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
                            MinimapIconFilePath = y.GetMinimapIconFilePath(),
                            AccountId = y.AccountId,
                            Denies = y.Denies,
                            Gold = y.Gold,
                            GoldPerMinute = y.GoldPerMinute,
                            RespawnTimer = y.RespawnTimer,
                            UltimateState = y.UltimateState,
                            LastHits = y.LastHits,
                            Level = y.Level,
                            NetWorth = y.NetWorth,
                            UltimateCooldown = y.UltimateCooldown,
                            XpPerMinute = y.XpPerMinute,
                            Items = new List<LiveLeagueGameItemViewModel>()
                            {
                                MakeGameItemViewModel(y.Item0),
                                MakeGameItemViewModel(y.Item1),
                                MakeGameItemViewModel(y.Item2),
                                MakeGameItemViewModel(y.Item3),
                                MakeGameItemViewModel(y.Item4),
                                MakeGameItemViewModel(y.Item5)
                            }.AsReadOnly()
                        })
                        .ToList()
                        .AsReadOnly(),
                RadiantPlayers = liveLeagueGame.Players
                        .Where(y => y.Team == 0)
                        .Select(y => new LiveLeagueGamePlayerViewModel()
                        {
                            HeroId = y.HeroId,
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
                            MinimapIconFilePath = y.GetMinimapIconFilePath(),
                            AccountId = y.AccountId,
                            Denies = y.Denies,
                            Gold = y.Gold,
                            GoldPerMinute = y.GoldPerMinute,
                            RespawnTimer = y.RespawnTimer,
                            UltimateState = y.UltimateState,
                            LastHits = y.LastHits,
                            Level = y.Level,
                            NetWorth = y.NetWorth,
                            UltimateCooldown = y.UltimateCooldown,
                            XpPerMinute = y.XpPerMinute,
                            Items = new List<LiveLeagueGameItemViewModel>()
                            {
                                MakeGameItemViewModel(y.Item0),
                                MakeGameItemViewModel(y.Item1),
                                MakeGameItemViewModel(y.Item2),
                                MakeGameItemViewModel(y.Item3),
                                MakeGameItemViewModel(y.Item4),
                                MakeGameItemViewModel(y.Item5)
                            }.AsReadOnly()
                        })
                        .ToList()
                        .AsReadOnly()
            };

            TimeSpan roshanRespawnTimer = TimeSpan.FromSeconds(liveLeagueGame.RoshanRespawnTimer);
            viewModel.RoshanRespawnTimer = String.Format("{0}m {1}s", roshanRespawnTimer.Minutes, roshanRespawnTimer.Seconds);

            TimeSpan streamDelay = TimeSpan.FromSeconds(liveLeagueGame.StreamDelay);
            viewModel.StreamDelay = String.Format("{0}m {1}s", streamDelay.Minutes, streamDelay.Seconds);

            return View(viewModel);
        }

        private LiveLeagueGameItemViewModel MakeGameItemViewModel(LiveLeagueGameItemModel gameItem)
        {
            if(gameItem != null)
            {
                return new LiveLeagueGameItemViewModel() { Id = gameItem.Id, Name = gameItem.Name, IconPath = gameItem.IconFileName };
            }
            else
            {
                return null;
            }
        }
    }
}