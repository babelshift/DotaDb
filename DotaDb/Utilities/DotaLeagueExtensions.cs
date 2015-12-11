using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.Utilities
{
    public static class DotaLeagueExtensions
    {
        public static string GetLogoFileName(this DotaLeague league)
        {
            if (String.IsNullOrEmpty(league.ImageBannerPath) || league.ImageBannerPath.EndsWith("ingame"))
            {
                return league.ImageInventoryPath.Replace("econ/leagues/", "") + ".jpg";
            }
            else
            {
                return league.ImageBannerPath.Replace("econ/leagues/", "") + ".jpg";
            }
        }
    }
}