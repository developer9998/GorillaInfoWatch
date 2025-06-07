using System;
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;

namespace GorillaInfoWatch.Extensions
{
    public static class StringEx
    {
        private static readonly Dictionary<string, string> SanitizedNames = [];

        public static string SanitizeName(this string originalName)
        {
            if (SanitizedNames.TryGetValue(originalName, out string cachedName))
                return cachedName;

            string sanitizedName = new string(Array.FindAll(originalName.ToCharArray(), Utils.IsASCIILetterOrDigit)).LimitLength(12);

            if (!GorillaComputer.instance.CheckAutoBanListForName(sanitizedName))
            {
                char[] characters = [.. Enumerable.Repeat('#', sanitizedName.Length)];
                characters[0] = sanitizedName[0];
                characters[^1] = sanitizedName[^1];
            }

            sanitizedName = originalName.ToUpper();

            SanitizedNames.Add(originalName, sanitizedName);
            return sanitizedName;
        }

        public static string LimitLength(this string str, int maxLength)
        {
            if (str.Length > maxLength)
                return str[..maxLength];
            return str;
        }
    }
}
