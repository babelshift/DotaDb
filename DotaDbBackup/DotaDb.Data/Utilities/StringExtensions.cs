using System;

namespace DotaDb.Data.Utilities
{
    public static class StringExtensions
    {
        public static string ToSlashSeparatedString(this string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Replace(" ", " / ");
                return value;
            }

            return String.Empty;
        }
    }
}