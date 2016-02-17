using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class LiveLeagueGameOverviewViewModel
    {
        public long MatchId { get; set; }
        public string LeagueName { get; set; }
        public int SpectatorCount { get; set; }
        public string RadiantTeamName { get; set; }
        public string DireTeamName { get; set; }
        public int RadiantKillCount { get; set; }
        public int DireKillCount { get; set; }
        public string ElapsedTime { get; set; }
        public int GameNumber { get; set; }
        public int BestOf { get; set; }
        public int RadiantSeriesWins { get; set; }
        public int DireSeriesWins { get; set; }
        public string LeagueLogoPath { get; set; }
        public string RadiantTeamLogo { get; set; }
        public string DireTeamLogo { get; set; }
        public TowerStateModel RadiantTowerStates { get; set; }
        public TowerStateModel DireTowerStates { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerViewModel> RadiantPlayers { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerViewModel> DirePlayers { get; set; }
    }
}