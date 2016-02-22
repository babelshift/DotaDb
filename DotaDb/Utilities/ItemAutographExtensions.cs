using Steam.Models.DOTA2;
using System;
using System.Configuration;
using System.IO;

namespace DotaDb.Utilities
{
    public static class ItemAutographExtensions
    {
        private static readonly string autographIconsBaseUrl = ConfigurationManager.AppSettings["autographIconsBaseUrl"].ToString();

        public static string GetIconPath(this SchemaItemAutographModel autograph)
        {
            if (autograph == null || String.IsNullOrEmpty(autograph.IconPath))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}.jpg?5", autographIconsBaseUrl, Path.GetFileName(autograph.IconPath));
        }
    }
}