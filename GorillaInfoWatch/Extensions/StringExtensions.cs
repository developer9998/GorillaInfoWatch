using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;

namespace GorillaInfoWatch.Extensions
{
    public static class StringExtensions
    {
        private static TextInfo TextInfo => cultureInfo.TextInfo;

        private static readonly CultureInfo cultureInfo = CultureInfo.InvariantCulture;

        private static readonly Dictionary<string, string> sanitizedNameCache = [];

        public static string SanitizeName(this string name)
        {
            name ??= string.Empty;

            if (sanitizedNameCache.TryGetValue(name, out string sanitizedName))
                return sanitizedName;

            VRRig localRig = (VRRig.LocalRig ?? GorillaTagger.Instance.offlineVRRig) ?? throw new InvalidOperationException("VRRig for local player is null");
            sanitizedName = localRig.NormalizeName(true, name);
            sanitizedNameCache.TryAdd(name, sanitizedName);
            return sanitizedName;
        }

        public static string ToTitleCase(this string original, bool forceLower = true) => TextInfo.ToTitleCase(forceLower ? original.ToLower() : original);

        public static string EnforceLength(this string str, int maxLength) => str.Length > maxLength ? str[..maxLength] : str;

        // TODO: allow for rich presence tags
        public static string[] ToTextArray(this string text, string prepend = null)
        {
            if (Main.Instance is null || Main.Instance.menuLines == null || Main.Instance.menuLines[0] is not WatchLine line)
                return [string.IsNullOrEmpty(prepend) ? text : string.Concat(prepend, text)];

            string originalText = line.Text.text;

            line.Text.text = text;
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
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
                    str.Append(charInfo.character);
                }

                lines[i] = i != 0 || string.IsNullOrEmpty(prepend) ? str.ToString() : string.Concat(prepend, str.ToString());
            }

            line.Text.text = originalText;
            line.Text.ForceMeshUpdate(true);

            return lines;
        }
    }
}
