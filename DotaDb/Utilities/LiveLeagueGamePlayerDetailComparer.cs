using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotaDb.Utilities
{
    public class LiveLeagueGamePlayerDetailComparer : IEqualityComparer<LiveLeagueGamePlayerDetailModel>
    {
        public bool Equals(LiveLeagueGamePlayerDetailModel player1, LiveLeagueGamePlayerDetailModel player2)
        {
            return player1.AccountId == player2.AccountId;
        }

        public int GetHashCode(LiveLeagueGamePlayerDetailModel player)
        {
            return player.AccountId.GetHashCode();
        }
    }
}
