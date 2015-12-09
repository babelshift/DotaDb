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
    }
}