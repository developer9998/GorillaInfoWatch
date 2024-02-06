using GorillaInfoWatch.Models;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Utilities
{
    public static class PresetUtils
    {
        public static Color Parse(PresetColours type)
        {
            if (type == PresetColours.Personalized) return GorillaTagger.Instance.offlineVRRig.playerColor;

            string html = type switch
            {
                PresetColours.Black => "000000",
                PresetColours.Grey => "7F7F7F",
                PresetColours.White => "FFFFFF",
                PresetColours.Red => "FF0000",
                PresetColours.Orange => "FF9000",
                PresetColours.Yellow => "FFFF00",
                PresetColours.Lime => "90FF00",
                PresetColours.Green => "00FF00",
                PresetColours.Mint => "00FF90",
                PresetColours.Teal => "00FFFF",
                PresetColours.Cyan => "0090FF",
                PresetColours.Blue => "0000FF",
                PresetColours.Purple => "9000FF",
                PresetColours.Pink => "FF00FF",
                _ => throw new NotImplementedException()
            };

            return ColorUtility.TryParseHtmlString(string.Concat("#", html, "ff"), out Color colour) ? colour : Color.black;
        }
    }
}
