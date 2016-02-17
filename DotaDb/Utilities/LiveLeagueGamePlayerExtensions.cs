using Steam.Models.DOTA2;
using System;
using System.Configuration;

namespace DotaDb.Utilities
{
    public static class LiveLeagueGamePlayerExtensions
    {
        private const int mapWidth = 15000;
        private const int mapCoordinateOffset = 7500;

        private static readonly string minimapIconsBaseUrl = ConfigurationManager.AppSettings["minimapIconsBaseUrl"].ToString();

        public static double GetPercentOfPositionValue(this double x)
        {
            return ((x + mapCoordinateOffset) / mapWidth) * 100;
        }

        public static string GetMinimapIconFilePath(this LiveLeagueGamePlayerModel player)
        {
            string fileName = !String.IsNullOrEmpty(player.HeroUrl) ? String.Format("{0}_icon.png", player.HeroUrl) : String.Empty;

            if (String.IsNullOrEmpty(fileName))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}", minimapIconsBaseUrl, fileName);
        }
    }
}