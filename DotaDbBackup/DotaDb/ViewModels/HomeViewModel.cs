using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class HomeViewModel
    {
        public uint InGamePlayerCount { get; set; }
        public uint DailyPeakPlayerCount { get; set; }
        public uint AllTimePeakPlayerCount { get; set; }
        public int HeroCount { get; set; }
        public int InGameItemCount { get; set; }
        public int ShopItemCount { get; set; }
        public int? LeagueCount { get; set; }
        public int HeroAbilityCount { get; set; }
        public int? LiveLeagueGameCount { get; set; }

        public LiveLeagueGameOverviewViewModel TopLiveLeagueGame { get; set; }

        public HeroViewModel RandomHero { get; set; }

        public IReadOnlyCollection<GameItemViewModel> RandomGameItems { get; set; }

        public IReadOnlyCollection<DotaBlogFeedItemViewModel> DotaBlogFeedItems { get; set; }
    }
}