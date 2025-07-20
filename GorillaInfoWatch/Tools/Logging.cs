using BepInEx.Logging;

namespace GorillaInfoWatch.Tools
{
    internal class Logging
    {
        private static ManualLogSource logSource;

        private static readonly bool debug = true;

        public Logging(ManualLogSource source)
        {
            logSource = source;
        }

        public static void Info(object message) => LogMessage(LogLevel.Info, message);

        public static void Message(object message) => LogMessage(LogLevel.Message, message);

        public static void Warning(object message) => LogMessage(LogLevel.Warning, message);

        public static void Error(object message) => LogMessage(LogLevel.Error, message);

        public static void Fatal(object message) => LogMessage(LogLevel.Fatal, message);

        public static void LogMessage(LogLevel level, object message)
        {
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            if (!Logging.debug || debug)
            {
                logSource.Log(level, message);
            }
        }
    }
}
