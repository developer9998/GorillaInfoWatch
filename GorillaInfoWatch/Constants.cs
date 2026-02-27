namespace GorillaInfoWatch
{
    public class Constants
    {
        public const string GUID = "dev.gorillainfowatch";
        public const string Name = "GorillaInfoWatch";
        public const string Version = "1.1.4";

        public const int SectionCapacity = 14;

        // TODO: make the menu tilt members configurable
        public const float MenuTiltAngle = 80; // calculated menu angle after this will start to visually tilt it
        public const float MenuTiltMinimum = 7.5f; // angle after previous
        public const float MenuTiltAmount = 0.95f; // angle factor after that

        public const string GorillaFriendsGUID = "net.rusjj.gorillafriends";

        public const string DataEntry_ShortcutName = "ShortcutName";
        public const string DataEntry_Consent = "Consent";

        #region Web Addresses

        public const string URL_MediaProcess = "https://github.com/developer9998/WindowsMediaController/releases/download/1.0.0/GorillaInfoMediaProcess.exe";

        public const string URL_ModVersion = "https://raw.githubusercontent.com/developer9998/GorillaInfoWatch/main/LatestVersion.txt";

        public const string URL_Supporters = "https://gworkers.soweli.uk/supporters";

        #endregion

        #region Assets

        public const string AssetBundlePath = "GorillaInfoWatch.Content.watchbundle";

        public const string AssetName_Content = "Content";
        // The majority of assets are stored in this content asset, including prefabs (watch/panel/keyboard) and collections (figures/items/symbols)

        #endregion

        #region Networking

        public const float Networking_RaiseInterval = 0.25f;
        public const string Networking_PropertyKey = "InfoWatch";

        public const string NetworkProperty_TimeOffset = "TimeOffset";
        public const string NetworkProperty_Orientation = "Orientation";

        public const string NetworkProperty_MediaTitle = "Title";
        public const string NetworkProperty_MediaArtist = "Artist";
        public const string NetworkProperty_MediaLength = "Length";

        #endregion

        public const string AnimatorProperty_IsLocal = "IsLocal";

        public const string AnimatorProperty_Tab = "Tab";
    }
}
