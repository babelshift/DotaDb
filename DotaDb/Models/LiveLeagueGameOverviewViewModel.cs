using Steam.Models.DOTA2;
using System.Collections.Generic;

namespace DotaDb.Models
{
    public class LiveLeagueGameOverviewViewModel
    {
        public ulong MatchId { get; set; }
        public string LeagueName { get; set; }
        public uint SpectatorCount { get; set; }
        public string RadiantTeamName { get; set; }
        public string DireTeamName { get; set; }
        public uint RadiantKillCount { get; set; }
        public uint DireKillCount { get; set; }
        public string ElapsedTime { get; set; }
        public uint GameNumber { get; set; }
        public uint BestOf { get; set; }
        public uint RadiantSeriesWins { get; set; }
        public uint DireSeriesWins { get; set; }
        public string LeagueLogoPath { get; set; }
        public string RadiantTeamLogo { get; set; }
        public string DireTeamLogo { get; set; }
        public TowerStateModel RadiantTowerStates { get; set; }
        public TowerStateModel DireTowerStates { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerViewModel> RadiantPlayers { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerViewModel> DirePlayers { get; set; }
    }
}