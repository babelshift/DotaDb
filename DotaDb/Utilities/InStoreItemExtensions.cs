using Steam.Models.DOTA2;
using System;

namespace DotaDb.Utilities
{
    public static class InStoreItemExtensions
    {
        public static string GetIconPath(this SchemaItem item, string baseUrl)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.DefIndex))
            {
                return string.Empty;
            }

            return $"{baseUrl}/{item.DefIndex}.jpg";
        }
    }
}