using System.Configuration;
using SteamWebAPI2.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Steam.Models.DOTA2;

namespace DotaDb.Utilities
{
    public static class GameItemExtensions
    {
        private static readonly string baseUrl = ConfigurationManager.AppSettings["itemIconsBaseUrl"].ToString();

        public static string GetIconPath(this GameItemModel item)
        {
            return String.Format("{0}{1}_lg.png", baseUrl, item.IsRecipe ? "recipe" : item.Name.Replace("item_", ""));
        }
    }
}