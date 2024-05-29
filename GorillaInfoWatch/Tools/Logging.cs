using BepInEx.Logging;
using Bepinject;
using Zenject;

namespace GorillaInfoWatch.Tools
{
    public class Logging : IInitializable
    {
        private static ManualLogSource Logger;

        [Inject]
        public void Construct(BepInLog logger)
        {
            Logger = logger.Logger;
        }

        public void Initialize()
        {
            Info("GorillaInfoWatch logging tool has succesfully initialized.");
        }

        public static void Info(object message) => Log(LogLevel.Info, message);

        public static void Warning(object message) => Log(LogLevel.Warning, message);

        public static void Error(object message) => Log(LogLevel.Error, message);

        private static void Log(LogLevel level, object message)
        {
#if DEBUG
            Logger.Log(level, message);
#endif
        }
    }
}
