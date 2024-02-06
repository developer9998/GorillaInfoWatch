using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Extensions
{
    public static class StringEx
    {
        private static Dictionary<string, string> FilteredDictionary = new();

        public static string GetFilteredName(this string str)
        {
            if (FilteredDictionary.TryGetValue(str, out string text)) return text;

            if (GorillaComputer.instance.CheckAutoBanListForName(str))
            {
                text = new string(Array.FindAll(text.ToCharArray().Select(c => char.ToUpper(c)).ToArray(), (char c) => char.IsLetterOrDigit(c)));
                if (text.Length > 12) text = text[..11];
            }
            else
            {
                text = "BADGORILLA";
            }

            FilteredDictionary.Add(str, text);
            return text;
        }

        public static string AlignRight(this string str, int width)
        {
            int padding = width - str.Length - 1;

            return string.Concat(Enumerable.Repeat(" ", padding)) + str;
        }

        public static string AlignCenter(this string str, int width)
        {
            int padding = Mathf.FloorToInt((width - str.Length) / 2f);

            string result = string.Concat(Enumerable.Repeat(" ", padding)) + str + string.Concat(Enumerable.Repeat(" ", padding));
            return result.Length > width ? result[..(width + 1)] : result;
        }

        public static string AlignCenter(this string str, int width, int fontScale) => AlignCenter(str, Mathf.FloorToInt(width * (Constants.FontSize / fontScale)));

        public static string AlignRight(this string str, int width, int fontScale) => AlignRight(str, Mathf.FloorToInt(width * (Constants.FontSize / fontScale)));
    }
}
