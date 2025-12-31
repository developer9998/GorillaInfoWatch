using BepInEx.Configuration;
using GorillaInfoWatch.Models;

namespace GorillaInfoWatch.Tools
{
    public class Configuration
    {
        public ConfigFile File;

        // General

        internal static ConfigEntry<WatchHand> Orientation;

        // Notifications

        internal static ConfigEntry<NotificationSource> AllowedNotifcationSources;

        // Shortcuts

        internal static ConfigEntry<float> ShortcutHoldDuration;

        internal static ConfigEntry<float> ShortcutInterval;

        // Privacy

        public static ConfigEntry<bool> ShowPublic;

        public static ConfigEntry<bool> ShowPrivate;

        public Configuration(ConfigFile file)
        {
            File = file;
            File.SaveOnConfigSet = true;

            Orientation = File.Bind("General", "Preferred Hand", WatchHand.Left, "Which hand your watch is placed on");

            AllowedNotifcationSources = File.Bind("Notifications", "Notification Sources", NotificationSource.All, "The list of sources you have allowed to send their respective notifications");

            // Use ShortcutHandler class to manage the value of this setting
            ShortcutHoldDuration = File.Bind("Shortcuts", "Shortcut Hold Duration", 0.25f, new ConfigDescription("The maximum duration needed to activate a shortcut using the shortcut button", new AcceptableValueRange<float>(0.25f, 1f), "Increment 0.25"));
            ShortcutInterval = File.Bind("Shortcuts", "Shortcut Inverval", 1f, new ConfigDescription("The minimum interval between shortcut activation using the shortcut button", new AcceptableValueRange<float>(0.5f, 2f), "Increment 0.25"));

            ShowPublic = File.Bind("Privacy", "Show Public Name", true, "Whether room names under public visibility are shown");
            ShowPrivate = File.Bind("Privacy", "Show Private Name", false, "Whether room names under private visibility are shown");
        }

        public enum WatchHand
        {
            Left = 1,
            Right = 0
        }
    }
}
