using GorillaNetworking;
using System;
using System.Collections.Generic;
namespace GorillaInfoWatch.Extensions
{
    public static class StringEx
    {
        private static readonly Dictionary<string, string> SanitizedNames = [];

        public static string NormalizeName(this string name)
        {
            if (SanitizedNames.ContainsKey(name)) return SanitizedNames[name];

			string original_name = name;

            if (GorillaComputer.instance.CheckAutoBanListForName(name))
			{
				name = new string(Array.FindAll(name.ToCharArray(), Utils.IsASCIILetterOrDigit)).LimitLength(12);
				name = name.ToUpper();
			}
			else
			{
				name = "BADGORILLA"; // scoreboard handles reporting, no need to do it here, or in any mod for that matter
			}

			SanitizedNames.Add(original_name, name);
			return name;
        }

		public static string LimitLength(this string str, int maxLength)
		{
			return (str.Length > maxLength) ? str[..maxLength] : str;
		}
    }
}
