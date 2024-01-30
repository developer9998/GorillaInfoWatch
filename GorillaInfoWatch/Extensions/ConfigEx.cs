using BepInEx.Configuration;
using GorillaInfoWatch.Utilities;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Extensions
{
    public static class ConfigEx
    {
        public static string ToDetailedString(this ConfigEntry<int> entry, int minValue, int maxValue) => string.Concat(entry.Definition.Key, ": ", entry.Value, " ", AsciiUtils.Bar(maxValue + Mathf.Abs(minValue), entry.Value + Mathf.Abs(minValue)));
        public static string ToDetailedString(this ConfigEntry<int> entry) => string.Concat(entry.Definition.Key, ": ", entry.Value);
        public static string ToDetailedString(this ConfigEntry<float> entry, float minValue, float maxValue) => string.Concat(entry.Definition.Key, ": ", Math.Round(entry.Value, 2), "", AsciiUtils.Bar(Mathf.FloorToInt(maxValue + Mathf.Abs(minValue)), Mathf.FloorToInt(entry.Value + Mathf.Abs(maxValue))));
        public static string ToDetailedString(this ConfigEntry<float> entry) => string.Concat(entry.Definition.Key, ": ", Math.Round(entry.Value, 2));
        public static string ToDetailedString(this ConfigEntry<object> entry) => string.Concat(entry.Definition.Key, ": ", entry.Value.ToString());
    }
}
