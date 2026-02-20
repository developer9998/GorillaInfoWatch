using System;
using System.Collections.Generic;
using System.Globalization;

namespace GorillaInfoWatch.Extensions;

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

    public static string LimitLength(this string str, int maxLength) => str.Length > maxLength ? str[..maxLength] : str;

    public static string EnforcePlayerNameLength(this string str) => str.LimitLength(12);
}
