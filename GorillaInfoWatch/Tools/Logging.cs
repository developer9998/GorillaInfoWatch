﻿using BepInEx.Logging;

namespace GorillaInfoWatch.Tools
{
    internal static class Logging
    {
        private static readonly bool DebugLogExclusive = true;

        public static void Debug(object message) => LogMessage(LogLevel.Debug, message);

        public static void Info(object message) => LogMessage(LogLevel.Info, message);

        public static void Warning(object message) => LogMessage(LogLevel.Warning, message);

        public static void Error(object message) => LogMessage(LogLevel.Error, message);

        public static void Fatal(object message) => LogMessage(LogLevel.Fatal, message);

        public static void LogMessage(LogLevel level, object message)
        {
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            if (!DebugLogExclusive || debug)
            {
                Plugin.PluginLogger.Log(level, message);
            }
        }
    }
}
