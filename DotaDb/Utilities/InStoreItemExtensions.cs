﻿using SourceSchemaParser.Dota2;
using System;
using System.Configuration;

namespace DotaDb.Utilities
{
    public static class InStoreItemExtensions
    {
        private static readonly string inStoreItemIconsBaseUrl = ConfigurationManager.AppSettings["inStoreItemIconsBaseUrl"].ToString();

        public static string GetIconPath(this DotaSchemaItem item)
        {
            if (item == null || String.IsNullOrEmpty(item.DefIndex))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}.jpg", inStoreItemIconsBaseUrl, item.DefIndex);
        }
    }
}