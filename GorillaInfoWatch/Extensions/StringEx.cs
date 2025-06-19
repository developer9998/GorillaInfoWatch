using Fusion;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.UI;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;

namespace GorillaInfoWatch.Extensions
{
    public static class StringEx
    {
        private static CultureInfo CultureInfo => CultureInfo.InvariantCulture;
        private static TextInfo TextInfo => CultureInfo.TextInfo;

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

        public static string LimitLength(this string original, int maxLength)
        {
            if (original.Length > maxLength)
                return original[..maxLength];
            return original;
        }

        public static string[] ToTextArray(this string original, string prepend = null)
        {
            if (!Main.HasInstance || Main.Instance.menu_lines == null || Main.Instance.menu_lines[0] is not InfoWatchLine line)
                return [string.IsNullOrEmpty(prepend) ? original : string.Concat(prepend, original)];

            string currentText = line.Text.text;

            line.Text.text = original;
            line.Text.ForceMeshUpdate(true);

            TMP_TextInfo textInfo = line.Text.textInfo;

            string[] lines = new string[textInfo.lineCount];

            for (int i = 0; i < textInfo.lineCount; i++)
            {
                TMP_LineInfo lineInfo = textInfo.lineInfo[i];
                int startCharIndex = lineInfo.firstCharacterIndex;
                int endCharIndex = startCharIndex + lineInfo.characterCount;

                StringBuilder str = new();

                for (int charIndex = startCharIndex; charIndex < endCharIndex; charIndex++)
                {
                    var charInfo = textInfo.characterInfo[charIndex];

                    // Append the rendered character (excluding any tags)
                    str.Append(charInfo.character);
                }

                lines[i] = i != 0 || string.IsNullOrEmpty(prepend) ? str.ToString() : string.Concat(prepend, str.ToString());
            }

            line.Text.text = currentText;
            line.Text.ForceMeshUpdate(true);

            return lines;
        }

        public static string ToTitleCase(this string original) => TextInfo.ToTitleCase(original.ToLower());
    }
}
