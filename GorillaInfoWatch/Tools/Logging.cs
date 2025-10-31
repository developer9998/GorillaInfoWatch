using BepInEx.Logging;

namespace GorillaInfoWatch.Tools;

internal class Logging
{
    private static ManualLogSource logSource;

    public Logging(ManualLogSource source) => logSource = source;

    public static void Fatal(object message) => Log(LogLevel.Fatal, message);

    public static void Error(object message) => Log(LogLevel.Error, message);

    public static void Warning(object message) => Log(LogLevel.Warning, message);

    public static void Message(object message) => Log(LogLevel.Message, message);

    public static void Info(object message) => Log(LogLevel.Info, message);

    private static void Log(LogLevel level, object message)
    {
#if DEBUG
        logSource.Log(level, message);
#endif
    }
}