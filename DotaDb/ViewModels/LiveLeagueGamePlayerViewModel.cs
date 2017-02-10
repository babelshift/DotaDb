using DotaDb.Data.Utilities;
using DotaDb.Utilities;
using System;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class LiveLeagueGamePlayerViewModel
    {
        public uint PlayerSlot { get; set; }
        public uint AccountId { get; set; }
        public string PlayerName { get; set; }
        public uint HeroId { get; set; }
        public string HeroName { get; set; }
        public string HeroAvatarFilePath { get; set; }
        public uint KillCount { get; set; }
        public uint DeathCount { get; set; }
        public uint AssistCount { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionXPercent { get; set; }
        public double PositionYPercent { get; set; }
        public string MinimapIconFilePath { get; set; }
        public IReadOnlyList<LiveLeagueGameItemViewModel> Items { get; set; }
        public uint NetWorth { get; set; }
        public string NetWorthDisplay
        {
            get
            {
                return FormatNumber(NetWorth);
            }
        }
        public uint Gold { get; set; }
        public string GoldDisplay
        {
            get
            {
                return FormatNumber(Gold);
            }
        }
        public uint Level { get; set; }
        public uint LastHits { get; set; }
        public uint Denies { get; set; }
        public uint GoldPerMinute { get; set; }
        public uint XpPerMinute { get; set; }
        public uint UltimateState { get; set; }
        public uint UltimateCooldown { get; set; }

        public uint XP
        {
            get
            {
                return HeroExperience.ToReachLevel(Level);
            }
        }

        public string XPDisplay
        {
            get
            {
                return FormatNumber(XP);
            }
        }

        public uint RespawnTimerSeconds { get; set; }
        public TimeSpan RespawnTimer { get { return TimeSpan.FromSeconds(RespawnTimerSeconds); } }
        public bool IsAlive { get { return RespawnTimerSeconds <= 0; } }

        private static string FormatNumber(uint num)
        {
            if (num >= 1000000)
            {
                return (num / 1000000D).ToString("0.#M");
            }
            if (num >= 1000)
            {
                return (num / 1000D).ToString("0.#k");
            }

            return num.ToString("#,0");
        }
    }
}