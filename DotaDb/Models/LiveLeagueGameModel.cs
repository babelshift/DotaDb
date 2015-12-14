﻿using System;
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
        public string ElapsedTimeDisplay { get; set; }
        public int GameNumber { get; set; }
        public int BestOf { get; set; }
        public int RadiantSeriesWins { get; set; }
        public int DireSeriesWins { get; set; }
        public string LeagueLogoPath { get; set; }
        public string RadiantTeamLogo { get; set; }
        public string DireTeamLogo { get; set; }
        public int RoshanRespawnTimer { get; set; }
        public long LobbyId { get; set; }
        public long MatchId { get; set; }
        public double StreamDelay { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerModel> Players { get; set; }
        public double ElapsedTime { get; internal set; }
    }
}