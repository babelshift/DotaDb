using Steam.Models.DOTA2;
using System;
using System.Configuration;

namespace DotaDb.Data.Utilities
{
    public static class LeagueModelExtensions
    {
        private static readonly string leagueImagesBaseUrl = ConfigurationManager.AppSettings["leagueImagesBaseUrl"].ToString();

        public static string GetLogoFilePath(this LeagueModel league)
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