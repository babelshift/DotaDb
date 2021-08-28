using Steam.Models.DOTA2;
using System;
using System.Configuration;

namespace DotaDb.Data.Utilities
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