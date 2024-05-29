using GorillaNetworking;
using System;
using System.Collections.Generic;
namespace GorillaInfoWatch.Extensions
{
    public static class NameSanitization
    {
        private static readonly Dictionary<string, string> SanitizedNames = [];

        public static string NormalizeName(this string name)
        {
            if (SanitizedNames.TryGetValue(name, out string text)) return text;

            if (GorillaComputer.instance.CheckAutoBanListForName(name))
            {
                text = new string(Array.FindAll(name.ToCharArray(), (char c) => char.IsLetterOrDigit(c)));
                if (text.Length > 12) text = text[..11];
            }
            else
            {
                text = "BADGORILLA";
            }

            SanitizedNames.Add(name, text);
            return text.ToUpper();
        }
    }
}
