using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DotaDb.Utilities
{
    public static class HeroExtensions
    {
        private static readonly string minimapIconsBaseUrl = ConfigurationManager.AppSettings["minimapIconsBaseUrl"].ToString();

        public static string GetMinimapIconFilePath(this DotaHeroSchemaItem hero)
        {
            if(hero == null)
            {
                return String.Empty;
            }

            string fileName = !String.IsNullOrEmpty(hero.Url) ? String.Format("{0}_icon.png", hero.Url) : String.Empty;

            if (String.IsNullOrEmpty(fileName))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}", minimapIconsBaseUrl, fileName);
        }
    }
}