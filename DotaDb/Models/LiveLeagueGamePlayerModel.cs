using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.Models
{
    public class LiveLeagueGamePlayerModel
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public int HeroId { get; set; }
        public string HeroName { get; set; }
        public string HeroAvatarImagePath { get; set; }
        public int Team { get; set; }
        public int KillCount { get; set; }
        public int DeathCount { get; set; }
        public int AssistCount { get; set; }
    }
}