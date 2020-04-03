using Steam.Models.DOTA2;
using System;
using System.Configuration;

namespace DotaDb.Data.Utilities
{
    public static class LiveLeagueGamePlayerExtensions
    {
        private const int mapWidth = 15000;
        private const int mapCoordinateOffset = 7500;

        private static readonly string minimapIconsBaseUrl = ConfigurationManager.AppSettings["minimapIconsBaseUrl"].ToString();

        /// <summary>
        /// Returns a percentage calculated from a player's current position on the DOTA 2 map so that we can accurately display the minimap icons of the player overlaying a map.
        /// For example, if the player is at position [X: 1500, Y: 3000], the following math is performed to get the percentage values.
        ///     ((X + Offset) / MapWidth) * 100
        ///     ((1500 + 7500) / 15000) * 100 = 60%
        ///     ((3000 + 7500) / 15000) * 100 = 70%
        ///     [X: 60%, Y: 70%]
        /// We offset by 7500 because it seems like the DOTA2 map coordinates are offset by that amount. The center of the map is not [0, 0] and is instead [7500, 7500].
        /// </summary>
        /// <param name="position">Position X or Position Y of the player's hero on the DOTA2 map</param>
        /// <returns>Percentage of where in the map (X or Y) the player's hero is located</returns>
        public static double GetPercentOfPositionValue(this double position)
        {
            return ((position + mapCoordinateOffset) / mapWidth) * 100;
        }

        /// <summary>
        /// Returns a minimap icon file path for a specific player's hero. The full URL is based on a configuration value to determine the base URL (such as Azure or AWS).
        /// </summary>
        /// <param name="player">Player in a live league game with a selected hero</param>
        /// <returns>URL path to wherever the minimap icon is located</returns>
        public static string GetMinimapIconFilePath(this LiveLeagueGamePlayerModel player)
        {
            string fileName = !String.IsNullOrWhiteSpace(player.HeroUrl) ? String.Format("{0}_icon.png", player.HeroUrl) : String.Empty;

            if (String.IsNullOrWhiteSpace(fileName))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}", minimapIconsBaseUrl, fileName);
        }
    }
}