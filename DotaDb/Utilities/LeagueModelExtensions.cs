using Steam.Models.DOTA2;
using System;

namespace DotaDb.Utilities
{
    public static class LeagueModelExtensions
    {
        public static string GetLogoFilePath(this LeagueModel league, string leagueImagesBaseUrl)
        {
            string fileName = String.Empty;

            if (String.IsNullOrEmpty(league.ImageBannerPath) || league.ImageBannerPath.EndsWith("ingame"))
            {
                fileName = league.ImageInventoryPath.Replace("econ/leagues/", "") + ".jpg";
            }
            else
            {
                fileName = league.ImageBannerPath.Replace("econ/leagues/", "") + ".jpg";
            }

            if (String.IsNullOrEmpty(fileName))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}", leagueImagesBaseUrl, fileName);
        }
    }
}