using Steam.Models.DOTA2;
using System;

namespace DotaDb.Utilities
{
    public static class GameItemExtensions
    {
        public static string GetIconPath(this GameItemModel item, string baseUrl)
        {
            return String.Format("{0}{1}_lg.png", baseUrl, item.IsRecipe ? "recipe" : item.Name.Replace("item_", ""));
        }
    }
}