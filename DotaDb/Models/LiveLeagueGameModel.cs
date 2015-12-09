using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.Models
{
    public class LiveLeagueGameModel
    {
        public string LeagueName { get; set; }
        public int SpectatorCount { get; set; }
        public string RadiantTeamName { get; set; }
        public string DireTeamName { get; set; }
        public int RadiantKillCount { get; set; }
        public int DireKillCount { get; set; }
        public string ElapsedTime { get; set; }
        public int GameNumber { get; set; }
        public int BestOf { get; set; }
        public string SeriesStatus { get; set; }
        public string LeagueLogo { get; set; }
        public string RadiantTeamLogo { get; set; }
        public string DireTeamLogo { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerModel> Players { get; set; }
    }
}