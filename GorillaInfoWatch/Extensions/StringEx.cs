using GorillaNetworking;
using System;
using System.Collections.Generic;
namespace GorillaInfoWatch.Extensions
{
    public static class StringEx
    {
        private static readonly Dictionary<string, string> FilteredDictionary = [];

        public static string ToNormalizedName(this string str)
        {
            if (FilteredDictionary.TryGetValue(str, out string text)) return text;

            if (GorillaComputer.instance.CheckAutoBanListForName(str))
            {
                text = new string(Array.FindAll(str.ToCharArray(), (char c) => char.IsLetterOrDigit(c)));
                if (text.Length > 12) text = text[..11];
            }
            else
            {
                text = "BADGORILLA";
            }

            FilteredDictionary.Add(str, text);
            return text.ToUpper();
        }
    }
}
