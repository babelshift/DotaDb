using System;

namespace DotaDb.Utilities
{
    public static class StringUtilities
    {
        public static string ToSlashSeparatedString(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.Replace(" ", " / ");
                return value;
            }

            return String.Empty;
        }

        public static string ReplaceUnderscoresWithSpaces(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? value.Replace("_", " ") : string.Empty;
        }
    }
}