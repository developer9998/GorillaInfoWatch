using MelonLoader;

namespace GorillaInfoWatch.Tools
{
    internal class Logging
    {
        private static MelonLogger.Instance logSource;

        public Logging(MelonLogger.Instance source)
        {
            logSource = source;
        }

        public static void Message(object data) => logSource.Msg(data);

        public static void Info(object data) => logSource.Msg(MelonLoader.Logging.ColorARGB.Gray, data);

        public static void Warning(object data) => logSource.Warning(data);

        public static void Error(object data) => logSource.Error(data);

        public static void Fatal(object data) => logSource.Error(data);
    }
}
