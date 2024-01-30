﻿using BepInEx.Configuration;
using System;
using System.Linq;
using UnityEngine;
using GorillaInfoWatch.Utilities;

namespace GorillaInfoWatch.Extensions
{
    public static class StringEx
    {
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

        public static string AlignCenter(this string str, int width, int fontScale) => AlignCenter(str, Mathf.FloorToInt(width * (10f / fontScale)));

        public static string AlignRight(this string str, int width, int fontScale) => AlignRight(str, Mathf.FloorToInt(width * (10f / fontScale)));
    }
}
