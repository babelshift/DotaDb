using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class HomeViewModel
    {
        public int InGamePlayerCount { get; set; }
        public int DailyPeakPlayerCount { get; set; }
        public int AllTimePeakPlayerCount { get; set; }
        public int HeroCount { get; set; }
        public int InGameItemCount { get; set; }
        public int ShopItemCount { get; set; }
        public int? LeagueCount { get; set; }
        public int HeroAbilityCount { get; set; }
        public int? LiveLeagueGameCount { get; set; }

        public IReadOnlyCollection<LiveLeagueGameOverviewViewModel> LiveLeagueGames { get; set; }

        public HeroViewModel RandomHero { get; set; }

        public IReadOnlyCollection<GameItemViewModel> RandomGameItems { get; set; }
    }
}