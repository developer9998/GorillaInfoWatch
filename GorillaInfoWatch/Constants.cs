namespace GorillaInfoWatch
{
    public class Constants
    {
        public const string Guid = "dev.gorillainfowatch";
        public const string Name = "GorillaInfoWatch";
        public const string Version = "1.0.0.0";

        public const bool DebugLogExclusive = false;

        public const int LinesPerPage = 13;

        public const float NetworkSetInterval = 0.5f;
        public const string NetworkVersionKey = "InfoWatchVersion";
        public const string NetworkPropertiesKey = "InfoWatchProperties";

        public const float MenuTiltAngle = 80; // calculated menu angle after this will start to visually tilt it
        public const float MenuTiltMinimum = 7.5f; // angle after previous
        public const float MenuTiltAmount = 0.95f; // angle factor after that
    }
}
