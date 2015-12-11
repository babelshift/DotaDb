using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DotaDb.Utilities
{
    public static class DotaLeagueExtensions
    {
        private static readonly string leagueImagesBaseUrl = ConfigurationManager.AppSettings["leagueImagesBaseUrl"].ToString();

        public static string GetLogoFilePath(this DotaLeague league)
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

            return String.Format("{0}{1}", leagueImagesBaseUrl, fileName);
        }
    }
}