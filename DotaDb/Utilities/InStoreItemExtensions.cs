using Steam.Models.DOTA2;
using System;

namespace DotaDb.Utilities
{
    public static class InStoreItemExtensions
    {
        public static string GetIconPath(this SchemaItemModel item, string baseUrl)
        {
            if (item == null || String.IsNullOrEmpty(item.DefIndex))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}.jpg", baseUrl, item.DefIndex);
        }
    }
}