using Azure.Storage.Blobs;
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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class LiveLeagueGamesService
    {
        private readonly IConfiguration configuration;
        private readonly ISchemaParser schemaParser;
        private readonly BlobServiceClient blobServiceClient;
        private readonly SteamWebInterfaceFactory steamWebInterfaceFactory;
        private readonly string itemIconsBaseUrl;
        private readonly string heroAvatarsBaseUrl;
        private readonly string minimapIconsBaseUrl;
        private readonly string leagueImagesBaseUrl;

        public LiveLeagueGamesService(
            IConfiguration configuration,
            ISchemaParser schemaParser,
            BlobServiceClient blobServiceClient)
        {
            this.configuration = configuration;
            this.schemaParser = schemaParser;
            this.blobServiceClient = blobServiceClient;
            string steamWebApiKey = configuration["SteamWebApiKey"];
            steamWebInterfaceFactory = new SteamWebInterfaceFactory(steamWebApiKey);
            itemIconsBaseUrl = configuration["ItemIconsBaseUrl"];
            heroAvatarsBaseUrl = configuration["HeroAvatarsBaseUrl"];
            minimapIconsBaseUrl = configuration["MinimapIconsBaseUrl"];
            leagueImagesBaseUrl = configuration["LeagueImagesBaseUrl"];
        }

        public async Task<LiveLeagueGameOverviewViewModel> GetTopLiveLeagueGameAsync()
        {
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
                GameNumber = topLiveLeagueGame.RadiantSeriesWins + topLiveLeagueGame.DireSeriesWins + 1,
                LeagueLogoPath = "Unknown", // lookup from league logos collections
                LeagueName = "Unknown", // lookup from league information
                RadiantKillCount = topLiveLeagueGame.Scoreboard?.Radiant?.Score ?? 0,
                RadiantTeamLogo = "Unknown", // lookup from logos collections
                RadiantTeamName = topLiveLeagueGame.RadiantTeam?.TeamName ?? "Unknown",
                RadiantSeriesWins = topLiveLeagueGame.RadiantSeriesWins,
                DireSeriesWins = topLiveLeagueGame.DireSeriesWins,
                SpectatorCount = topLiveLeagueGame.Spectators,
                RadiantTowerStates = topLiveLeagueGame.Scoreboard?.Radiant?.TowerStates,
                DireTowerStates = topLiveLeagueGame.Scoreboard?.Dire?.TowerStates,
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

            var heroes = await GetHeroesFromSchemaAsync();

            // for all the players in this game, try to fill in their details, stats, names, etc.
            foreach (var player in players)
            {
                // skip over spectators/observers/commentators
                if (player.Team != 0 && player.Team != 1)
                {
                    continue;
                }

                var hero = heroes[player.HeroId];
                player.HeroName = hero.Name;
                player.HeroAvatarImageFilePath = string.Empty; // TODO: implement extension method to get image path
                player.HeroUrl = hero.Url;

                LiveLeagueGamePlayerDetailModel playerDetail = GetPlayerDetailForLiveLeagueGame(player.Team, player.AccountId, radiantPlayerDetail, direPlayerDetail);

                // if the player hasn't picked a hero yet, details won't exist
                if (playerDetail != null)
                {
                    player.KillCount = playerDetail.Kills;
                    player.DeathCount = playerDetail.Deaths;
                    player.AssistCount = playerDetail.Assists;
                    player.PositionX = playerDetail.PositionX;
                    player.PositionY = playerDetail.PositionY;
                    player.NetWorth = playerDetail.NetWorth;
                    player.Level = playerDetail.Level;
                }
            }

            response.DirePlayers = players
                .Where(x => x.Team == 1)
                .Select(x => new LiveLeagueGamePlayerViewModel()
                {
                    HeroName = x.HeroName,
                    HeroAvatarFilePath = x.HeroAvatarImageFilePath,
                    PlayerName = x.Name,
                    DeathCount = x.DeathCount,
                    KillCount = x.KillCount,
                    AssistCount = x.AssistCount,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    PositionXPercent = x.PositionX.GetPercentOfPositionValue(),
                    PositionYPercent = x.PositionY.GetPercentOfPositionValue(),
                    MinimapIconFilePath = x.GetMinimapIconFilePath(minimapIconsBaseUrl)
                }).ToList();

            response.RadiantPlayers = players
                .Where(x => x.Team == 0)
                .Select(x => new LiveLeagueGamePlayerViewModel()
                {
                    HeroName = x.HeroName,
                    HeroAvatarFilePath = x.HeroAvatarImageFilePath,
                    PlayerName = x.Name,
                    DeathCount = x.DeathCount,
                    KillCount = x.KillCount,
                    AssistCount = x.AssistCount,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    PositionXPercent = x.PositionX.GetPercentOfPositionValue(),
                    PositionYPercent = x.PositionY.GetPercentOfPositionValue(),
                    MinimapIconFilePath = x.GetMinimapIconFilePath(minimapIconsBaseUrl)
                }).ToList();

            return response;
        }

        private async Task<IReadOnlyDictionary<string, LeagueModel>> GetLeagueTicketsAsync()
        {
            var schemaVdf = await GetFileFromStorageAsync("schemas", "items_game.vdf");
            var localizationVdf = await GetFileFromStorageAsync("schemas", "items_english.vdf");
            var leagues = schemaParser.GetDotaLeaguesFromText(schemaVdf, localizationVdf);
            return new ReadOnlyDictionary<string, LeagueModel>(leagues.ToDictionary(x => x.ItemDef.ToString(), x => x));
        }

        private async Task<IReadOnlyCollection<GameItemModel>> GetGameItemsAsync()
        {
            var dota2Econ = steamWebInterfaceFactory.CreateSteamWebInterface<DOTA2Econ>(new HttpClient());
            var gameItems = await dota2Econ.GetGameItemsAsync();
            return gameItems.Data;
        }

        private static LiveLeagueGamePlayerDetailModel GetPlayerDetailForLiveLeagueGame(
            uint playerTeam, uint playerAccountId,
            IDictionary<uint, LiveLeagueGamePlayerDetailModel> radiantPlayerDetail,
            IDictionary<uint, LiveLeagueGamePlayerDetailModel> direPlayerDetail)
        {
            // team 0 is radiant, if the player hasn't picked a hero yet, details won't exist
            if (playerTeam == 0)
            {
                if (radiantPlayerDetail != null)
                {
                    LiveLeagueGamePlayerDetailModel playerDetail = null;
                    radiantPlayerDetail.TryGetValue(playerAccountId, out playerDetail);
                    return playerDetail;
                }
            }
            // team 1 is dire
            else if (playerTeam == 1)
            {
                if (direPlayerDetail != null)
                {
                    LiveLeagueGamePlayerDetailModel playerDetail = null;
                    direPlayerDetail.TryGetValue(playerAccountId, out playerDetail);
                    return playerDetail;
                }
            }

            return null;
        }

        private async Task<IReadOnlyDictionary<uint, HeroSchemaModel>> GetHeroesFromSchemaAsync()
        {
            var vdf = await GetFileFromStorageAsync("schemas", "heroes.vdf");
            var heroes = schemaParser.GetDotaHeroes(vdf);
            return heroes.ToDictionary(x => x.HeroId, x => x);
        }

        private async Task<IReadOnlyCollection<string>> GetFileFromStorageAsync(string containerName, string fileName)
        {
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            var download = await blobClient.DownloadAsync();

            List<string> contents = new List<string>();
            using (StreamReader sr = new StreamReader(download.Value.Content))
            {
                while (!sr.EndOfStream)
                {
                    string line = await sr.ReadLineAsync();
                    contents.Add(line);
                }
            }

            return new ReadOnlyCollection<string>(contents);
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