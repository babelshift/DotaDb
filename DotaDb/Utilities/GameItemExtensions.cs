using System.Configuration;
using SteamWebAPI2.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.Utilities
{
    public static class GameItemExtensions
    {
        private static readonly string baseUrl = ConfigurationManager.AppSettings["itemIconsBaseUrl"].ToString();

        public static string GetIconPath(this GameItem item)
        {
            return String.Format("{0}{1}_lg.png", baseUrl, item.Recipe == 1 ? "recipe" : item.Name.Replace("item_", ""));
        }
    }
}