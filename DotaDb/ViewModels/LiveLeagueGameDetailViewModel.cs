using DotaDb.Utilities;
using SteamWebAPI2.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotaDb.ViewModels
{
    public class LiveLeagueGameDetailViewModel
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
        public bool IsRoshanAlive { get; set; }
        public string RoshanRespawnTimer { get; set; }
        public long LobbyId { get; set; }
        public long MatchId { get; set; }
        public string StreamDelay { get; set; }
        public DateTime TimeStarted { get { return DateTime.Now - TimeSpan.FromSeconds(ElapsedTime); } }
        public int RadiantTotalWorth
        {
            get
            {
                return RadiantPlayers.Sum(x => x.NetWorth);
            }
        }
        public int DireTotalWorth
        {
            get
            {
                return DirePlayers.Sum(x => x.NetWorth);
            }
        }
        public double RadiantTotalWorthPercent
        {
            get
            {
                double radiantTotalWorth = RadiantTotalWorth;
                double direTotalWorth = DireTotalWorth; 
                return (radiantTotalWorth / (radiantTotalWorth + direTotalWorth)) * 100;
            }
        }
        public double DireTotalWorthPercent
        {
            get
            {
                double radiantTotalWorth = RadiantTotalWorth;
                double direTotalWorth = DireTotalWorth;
                return (direTotalWorth / (radiantTotalWorth + direTotalWorth)) * 100;
            }
        }
        public int RadiantTotalExperience
        {
            get
            {
                return RadiantPlayers.Sum(x => HeroExperience.ToReachLevel(x.Level));
            }
        }
        public int DireTotalExperience
        {
            get
            {
                return DirePlayers.Sum(x => HeroExperience.ToReachLevel(x.Level));
            }
        }
        public double RadiantTotalExperiencePercent
        {
            get
            {
                double radiantTotalExperience = RadiantTotalExperience;
                double direTotalExperience = DireTotalExperience;
                return (radiantTotalExperience / (radiantTotalExperience + direTotalExperience)) * 100;
            }
        }
        public double DireTotalExperiencePercent
        {
            get
            {
                double radiantTotalExperience = RadiantTotalExperience;
                double direTotalExperience = DireTotalExperience;
                return (direTotalExperience / (radiantTotalExperience + direTotalExperience)) * 100;
            }
        }
        public IReadOnlyCollection<LiveLeagueGamePlayerViewModel> RadiantPlayers { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerViewModel> DirePlayers { get; set; }
        public double ElapsedTime { get; internal set; }
        public TowerState RadiantTowerStates { get; set; }
        public TowerState DireTowerStates { get; set; }
        public IReadOnlyList<LiveLeagueGameHeroViewModel> RadiantPickedHeroes { get; set; }
        public IReadOnlyList<LiveLeagueGameHeroViewModel> RadiantBannedHeroes { get; set; }
        public IReadOnlyList<LiveLeagueGameHeroViewModel> DirePickedHeroes { get; set; }
        public IReadOnlyList<LiveLeagueGameHeroViewModel> DireBannedHeroes { get; set; }

    }
}