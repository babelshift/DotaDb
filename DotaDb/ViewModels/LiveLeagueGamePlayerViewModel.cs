namespace DotaDb.ViewModels
{
    public class LiveLeagueGamePlayerViewModel
    {
        public string PlayerName { get; set; }
        public string HeroName { get; set; }
        public string HeroAvatarImagePath { get; set; }
        public int KillCount { get; set; }
        public int DeathCount { get; set; }
        public int AssistCount { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionXPercent { get; set; }
        public double PositionYPercent { get; set; }
        public string MinimapIconFileName { get; set; }
    }
}