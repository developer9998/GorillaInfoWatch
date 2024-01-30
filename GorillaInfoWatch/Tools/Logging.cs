using BepInEx.Logging;

namespace GorillaInfoWatch.Tools
{
    public class Logging
    {
        private static ManualLogSource Logger;

        public Logging(ManualLogSource logger)
        {
            Logger = logger;
        }

        public static void Info(object message) => Log(LogLevel.Info, message);

        public static void Warning(object message) => Log(LogLevel.Warning, message);

        public static void Error(object message) => Log(LogLevel.Error, message);

        private static void Log(LogLevel level, object message) => Logger.Log(level, message);
    }
}
