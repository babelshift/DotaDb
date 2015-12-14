using DotaDb.Utilities;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class LiveLeagueGamePlayerViewModel
    {
        public int PlayerSlot { get; set; }
        public int AccountId { get; set; }
        public string PlayerName { get; set; }
        public int HeroId { get; set; }
        public string HeroName { get; set; }
        public string HeroAvatarFileName { get; set; }
        public int KillCount { get; set; }
        public int DeathCount { get; set; }
        public int AssistCount { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionXPercent { get; set; }
        public double PositionYPercent { get; set; }
        public string MinimapIconFilePath { get; set; }
        public IReadOnlyList<LiveLeagueGameItemViewModel> Items { get; set; }
        public int RespawnTimer { get; set; }
        public int NetWorth { get; set; }
        public int Gold { get; set; }
        public int Level { get; set; }
        public int LastHits { get; set; }
        public int Denies { get; set; }
        public int GoldPerMinute { get; set; }
        public int XpPerMinute { get; set; }
        public int UltimateState { get; set; }
        public int UltimateCooldown { get; set; }
        public int XP
        {
            get
            {
                return HeroExperience.ToReachLevel(Level);
            }
        }
    }
}