using DotaDb.Models;
using DotaDb.Utilities;
using Microsoft.Extensions.Configuration;
using Steam.Models.DOTA2;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class LiveLeagueGamesService
    {
        private readonly IConfiguration configuration;

        public LiveLeagueGamesService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private async Task<LiveLeagueGameOverviewViewModel> GetTopLiveLeagueGameAsync()
        {
            string steamWebApiKey = configuration["steamWebApiKey"];
            var steamWebInterfaceFactory = new SteamWebInterfaceFactory(steamWebApiKey);
            
            // TODO: http client factory injection?
            var dota2MatchInterface = steamWebInterfaceFactory.CreateSteamWebInterface<DOTA2Match>(new HttpClient());
            
            var liveLeagueGames = await dota2MatchInterface.GetLiveLeagueGamesAsync();
            var sortedLiveLeagueGames = liveLeagueGames.Data
                .OrderByDescending(x => x.Spectators)
                .AsEnumerable();
            var topLiveLeagueGame = sortedLiveLeagueGames.ElementAt(0);

            var response = new LiveLeagueGameOverviewViewModel()
            {
                MatchId = topLiveLeagueGame.MatchId,
                BestOf = GetBestOfCountFromSeriesType(topLiveLeagueGame.SeriesType),
                DireKillCount = topLiveLeagueGame.Scoreboard?.Dire?.Score ?? 0,
                DireTeamLogo = "Unknown", // lookup from logos collections
                DireTeamName = topLiveLeagueGame.DireTeam?.TeamName ?? "Unknown",
                ElapsedTime = topLiveLeagueGame.Scoreboard != null ? GetElapsedTime(topLiveLeagueGame.Scoreboard.Duration) : "Unknown",
                GameNumber = topLiveLeagueGame.GameNumber,
                LeagueLogoPath = "Unknown", // lookup from league logos collections
                LeagueName = "Unknown", // lookup from league information
                RadiantKillCount = topLiveLeagueGame.Scoreboard?.Radiant?.Score ?? 0,
                RadiantTeamLogo = "Unknown", // lookup from logos collections
                RadiantTeamName = topLiveLeagueGame.RadiantTeam?.TeamName ?? "Unknown",
                RadiantSeriesWins = topLiveLeagueGame.RadiantSeriesWins,
                DireSeriesWins = topLiveLeagueGame.DireSeriesWins,
                SpectatorCount = topLiveLeagueGame.Spectators,
                RadiantTowerStates = topLiveLeagueGame.Scoreboard?.Radiant?.TowerStates,
                DireTowerStates = topLiveLeagueGame.Scoreboard?.Dire?.TowerStates
            };

            var players = topLiveLeagueGame.Players
                .Select(x => new LiveLeagueGamePlayerModel()
                {
                    AccountId = x.AccountId,
                    HeroId = x.HeroId,
                    Name = x.Name,
                    Team = x.Team
                });

            Dictionary<uint, LiveLeagueGamePlayerDetailModel> radiantPlayerDetail = null;
            Dictionary<uint, LiveLeagueGamePlayerDetailModel> direPlayerDetail = null;

            // if the game hasn't started yet, the scoreboard won't exist
            // filter out duplicate player info
            if (topLiveLeagueGame.Scoreboard != null)
            {
                radiantPlayerDetail = topLiveLeagueGame.Scoreboard.Radiant.Players
                    .Distinct(new LiveLeagueGamePlayerDetailComparer())
                    .ToDictionary(x => x.AccountId, x => x);
                direPlayerDetail = topLiveLeagueGame.Scoreboard.Dire.Players
                    .Distinct(new LiveLeagueGamePlayerDetailComparer())
                    .ToDictionary(x => x.AccountId, x => x);
            }

            // for all the players in this game, try to fill in their details, stats, names, etc.
            foreach (var player in players)
            {
                // skip over spectators/observers/commentators
                if (player.Team != 0 && player.Team != 1)
                {
                    continue;
                }

                //var hero = await GetHeroKeyValueAsync(player.HeroId);
                //player.HeroName = await GetLocalizationTextAsync(hero.Name);
                //player.HeroAvatarImageFilePath = hero.GetAvatarImageFilePath();
                //player.HeroUrl = hero.Url;

                //LiveLeagueGamePlayerDetailModel playerDetail = GetPlayerDetailForLiveLeagueGame(player.Team, player.AccountId, radiantPlayerDetail, direPlayerDetail);

                //// if the player hasn't picked a hero yet, details won't exist
                //if (playerDetail != null)
                //{
                //    player.KillCount = playerDetail.Kills;
                //    player.DeathCount = playerDetail.Deaths;
                //    player.AssistCount = playerDetail.Assists;
                //    player.PositionX = playerDetail.PositionX;
                //    player.PositionY = playerDetail.PositionY;
                //    player.NetWorth = playerDetail.NetWorth;
                //    player.Level = playerDetail.Level;
                //}
            }


            //var liveLeagueGameViewModels = liveLeagueGames
            //    .Select(x => new LiveLeagueGameOverviewViewModel()
            //    {
            //        MatchId = x.MatchId,
            //        BestOf = x.BestOf,
            //        DireKillCount = x.DireKillCount,
            //        DireTeamLogo = x.DireTeamLogo,
            //        DireTeamName = x.DireTeamName,
            //        ElapsedTime = x.ElapsedTimeDisplay,
            //        GameNumber = x.GameNumber,
            //        LeagueLogoPath = x.LeagueLogoPath,
            //        LeagueName = x.LeagueName,
            //        RadiantKillCount = x.RadiantKillCount,
            //        RadiantTeamLogo = x.RadiantTeamLogo,
            //        RadiantTeamName = x.RadiantTeamName,
            //        RadiantSeriesWins = x.RadiantSeriesWins,
            //        DireSeriesWins = x.DireSeriesWins,
            //        SpectatorCount = x.SpectatorCount,
            //        RadiantTowerStates = x.RadiantTowerStates,
            //        DireTowerStates = x.DireTowerStates,
            //        DirePlayers = x.Players
            //            .Where(y => y.Team == 1)
            //            .Select(y => new LiveLeagueGamePlayerViewModel()
            //            {
            //                HeroName = y.HeroName,
            //                HeroAvatarFilePath = y.HeroAvatarImageFilePath,
            //                PlayerName = y.Name,
            //                DeathCount = y.DeathCount,
            //                KillCount = y.KillCount,
            //                AssistCount = y.AssistCount,
            //                PositionX = y.PositionX,
            //                PositionY = y.PositionY,
            //                PositionXPercent = y.PositionX.GetPercentOfPositionValue(),
            //                PositionYPercent = y.PositionY.GetPercentOfPositionValue(),
            //                MinimapIconFilePath = y.GetMinimapIconFilePath()
            //            })
            //            .ToList()
            //            .AsReadOnly(),
            //        RadiantPlayers = x.Players
            //            .Where(y => y.Team == 0)
            //            .Select(y => new LiveLeagueGamePlayerViewModel()
            //            {
            //                HeroName = y.HeroName,
            //                HeroAvatarFilePath = y.HeroAvatarImageFilePath,
            //                PlayerName = y.Name,
            //                DeathCount = y.DeathCount,
            //                KillCount = y.KillCount,
            //                AssistCount = y.AssistCount,
            //                PositionX = y.PositionX,
            //                PositionY = y.PositionY,
            //                PositionXPercent = y.PositionX.GetPercentOfPositionValue(),
            //                PositionYPercent = y.PositionY.GetPercentOfPositionValue(),
            //                MinimapIconFilePath = y.GetMinimapIconFilePath()
            //            })
            //            .ToList()
            //            .AsReadOnly()
            //    })
            //    .ToList()
            //    .AsReadOnly();

            //if (liveLeagueGameViewModels != null && liveLeagueGameViewModels.Count > 0)
            //{
            //    return liveLeagueGameViewModels[0];
            //}
            //else
            //{
            //    return null;
            //}

            return response;
        }

        private static uint GetBestOfCountFromSeriesType(uint seriesType)
        {
            if (seriesType == 0)
            {
                return 1;
            }
            else if (seriesType == 1)
            {
                return 3;
            }
            else if (seriesType == 2)
            {
                return 5;
            }
            else
            {
                return 0;
            }
        }

        private static string GetElapsedTime(double seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return String.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}
