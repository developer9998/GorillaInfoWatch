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

        internal static ConfigEntry<float> NotifHapticAmplitude;

        internal static ConfigEntry<float> NotifHapticDuration;

        internal static ConfigEntry<float> SilentNotifHapticAmplitude;

        internal static ConfigEntry<float> SilentNotifHapticDuration;

        internal static ConfigEntry<float> NotificationSoundVolume;

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
            NotifHapticAmplitude = File.Bind("Notifications", "Notification Haptic Amplitude", 0.04f, new ConfigDescription("The amplitude of the haptic sent when a notification is recieved", new AcceptableValueRange<float>(0.01f, 0.2f)));
            NotifHapticDuration = File.Bind("Notifications", "Notification Haptic Duration", 0.2f, new ConfigDescription("The duration of the haptic sent when a notification is recieved", new AcceptableValueRange<float>(0.05f, 0.3f)));
            SilentNotifHapticAmplitude = File.Bind("Notifications", "Silent Notification Haptic Amplitude", 0.2f, new ConfigDescription("The amplitude of the haptic sent when a silent notification is recieved", new AcceptableValueRange<float>(0.01f, 0.2f)));
            SilentNotifHapticDuration = File.Bind("Notifications", "Silent Notification Haptic Duration", 0.1f, new ConfigDescription("The duration of the haptic sent when a silent notification is recieved", new AcceptableValueRange<float>(0.05f, 0.3f)));
            NotificationSoundVolume = File.Bind("Notifications", "Notification Sound Volume", 1f, new ConfigDescription("The volume of the sound produced when a notification is recieved", new AcceptableValueRange<float>(0f, 1f)));

            // Use ShortcutHandler class to manage the value of this setting
            ShortcutHoldDuration = File.Bind("Shortcuts", "Shortcut Hold Duration", 0.25f, new ConfigDescription("The maximum duration needed to activate a shortcut using the shortcut button", new AcceptableValueRange<float>(0.25f, 1f)));
            ShortcutInterval = File.Bind("Shortcuts", "Shortcut Inverval", 1f, new ConfigDescription("The minimum interval between shortcut activation using the shortcut button", new AcceptableValueRange<float>(0.5f, 2f)));

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
