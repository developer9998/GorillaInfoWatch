using System;
using GameObjectScheduling;

namespace GorillaInfoWatch.Extensions
{
    public static class TimeEx
    {
        public static string GetTimeDisplay(this DateTime dateTime)
        {
            string displayTextFormat = "{0} {1}";
            return CountdownText.GetTimeDisplay(TimeSpan.FromTicks(dateTime.Ticks), displayTextFormat);
        }
    }
}
