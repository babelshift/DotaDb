using SteamWebAPI2.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.Utilities
{
    public class LiveLeagueGamePlayerDetailComparer : IEqualityComparer<LiveLeagueGamePlayerDetail>
    {
        public bool Equals(LiveLeagueGamePlayerDetail player1, LiveLeagueGamePlayerDetail player2)
        {
            return player1.AccountId == player2.AccountId;
        }
        public int GetHashCode(LiveLeagueGamePlayerDetail player)
        {
            return player.GetHashCode();
        }
    }
}