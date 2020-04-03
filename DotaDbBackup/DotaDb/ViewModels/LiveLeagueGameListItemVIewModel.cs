using DotaDb.Data.Utilities;
using DotaDb.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace DotaDb.ViewModels
{
    public class LiveLeagueGameListItemViewModel
    {
        public ulong MatchId { get; set; }
        public string LeagueName { get; set; }
        public string LeagueTier { get; set; }
        public string RadiantTeamName { get; set; }
        public string DireTeamName { get; set; }
        public string LeagueLogoPath { get; set; }
        public string ElapsedTime { get; set; }
        public uint SpectatorCount { get; set; }
        public uint BestOf { get; set; }
        public uint RadiantSeriesWins { get; set; }
        public uint DireSeriesWins { get; set; }
        public uint RadiantKillCount { get; set; }
        public uint DireKillCount { get; set; }
        public string RadiantTeamLogo { get; set; }
        public string DireTeamLogo { get; set; }

        public uint RadiantTotalWorth
        {
            get
            {
                return (uint)RadiantPlayers.Sum(x => x.NetWorth);
            }
        }

        public uint DireTotalWorth
        {
            get
            {
                return (uint)DirePlayers.Sum(x => x.NetWorth);
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

        public uint RadiantTotalExperience
        {
            get
            {
                return (uint)RadiantPlayers.Sum(x => HeroExperience.ToReachLevel(x.Level));
            }
        }

        public uint DireTotalExperience
        {
            get
            {
                return (uint)DirePlayers.Sum(x => HeroExperience.ToReachLevel(x.Level));
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