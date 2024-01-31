using GorillaInfoWatch.Models;
using GorillaInfoWatch.Utilities;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Extensions
{
    // https://github.com/ToniMacaroni/ComputerInterface/blob/50468f20b4bb7e755d933f8c63627d8cf9394a0e/ComputerInterface/StringBuilderEx.cs
    public static class StringBuilderEx
    {
        public static StringBuilder AppendClr(this StringBuilder str, string text, string color) => str.BeginColor(color).Append(text).EndColor();

        public static StringBuilder BeginColor(this StringBuilder str, string color)
        {
            if (color[0] != '#') color = "#" + color;
            return str.Append($"<color={color}>");
        }

        public static StringBuilder BeginColor(this StringBuilder str, Color color) => str.BeginColor(ColorUtility.ToHtmlStringRGB(color));

        public static StringBuilder EndColor(this StringBuilder str) => str.Append("</color>");

        public static StringBuilder Repeat(this StringBuilder str, string toRepeat, int repeatNum)
        {
            for (int i = 0; i < repeatNum; i++)
            {
                str.Append(toRepeat);
            }

            return str;
        }

        public static StringBuilder AppendLines(this StringBuilder str, int numOfLines)
        {
            str.Repeat("\n", numOfLines);
            return str;
        }

        public static StringBuilder AppendSize(this StringBuilder str, string text, int size)
        {
            str.Append($"<size={size}%>").Append(text).Append("</size>");
            return str;
        }

        public static StringBuilder AppendBar(this StringBuilder str, int count, int length) => str.Append(AsciiUtils.Bar(length, count));

        public static StringBuilder AppendItem(this StringBuilder str, string text, int index, ItemHandler itemHandler) => str.Append(itemHandler.CurrentEntry == index ? " > " : "   ").Append(text).AppendLine();
    }
}
