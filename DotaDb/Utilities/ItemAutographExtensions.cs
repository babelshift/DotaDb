using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace DotaDb.Utilities
{
    public static class ItemAutographExtensions
    {
        private static readonly string autographIconsBaseUrl = ConfigurationManager.AppSettings["autographIconsBaseUrl"].ToString();

        public static string GetIconPath(this DotaSchemaItemAutograph autograph)
        {
            if (autograph == null || String.IsNullOrEmpty(autograph.IconPath))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}.jpg", autographIconsBaseUrl, Path.GetFileName(autograph.IconPath));
        }
    }
}