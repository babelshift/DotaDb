using DotaDb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class LiveLeagueGameListItemViewModel
    {
        public long MatchId { get; set; }
        public string LeagueName { get; set; }
        public string RadiantTeamName { get; set; }
        public string DireTeamName { get; set; }
        public string LeagueLogoPath { get; set; }
        public string ElapsedTime { get; set; }
        public int SpectatorCount { get; set; }
        public int BestOf { get; set; }
        public int RadiantSeriesWins { get; set; }
        public int DireSeriesWins { get; set; }
        public int RadiantKillCount { get; set; }
        public int DireKillCount { get; set; }

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
    }
}