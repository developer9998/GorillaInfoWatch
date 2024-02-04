using GorillaInfoWatch.Models;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Utilities
{
    public class PresetUtils
    {
        public static Color Parse(PresetColourTypes type)
        {
            string html = type switch
            {
                PresetColourTypes.Black => "000000",
                PresetColourTypes.Grey => "7F7F7F",
                PresetColourTypes.White => "FFFFFF",
                PresetColourTypes.Red => "FF0000",
                PresetColourTypes.Orange => "FF9000",
                PresetColourTypes.Yellow => "FFFF00",
                PresetColourTypes.Lime => "90FF00",
                PresetColourTypes.Green => "00FF00",
                PresetColourTypes.Mint => "00FF90",
                PresetColourTypes.Teal => "00FFFF",
                PresetColourTypes.Cyan => "0090FF",
                PresetColourTypes.Blue => "0000FF",
                PresetColourTypes.Purple => "9000FF",
                PresetColourTypes.Pink => "FF00FF",
                _ => throw new NotImplementedException()
            };

            return ColorUtility.TryParseHtmlString(string.Concat("#", html, "ff"), out Color colour) ? colour : Color.black;
        }
    }
}
