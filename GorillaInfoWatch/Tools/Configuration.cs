using GorillaInfoWatch.Models;
using MelonLoader;
using MelonLoader.Preferences;

namespace GorillaInfoWatch.Tools
{
    public class Configuration
    {
        public MelonPreferences_Category File;

        // General

        internal static MelonPreferences_Entry<WatchHand> Orientation;

        // Notifications

        internal static MelonPreferences_Entry<NotificationSource> AllowedNotifcationSources;

        internal static MelonPreferences_Entry<float> NotifHapticAmplitude;

        internal static MelonPreferences_Entry<float> NotifHapticDuration;

        internal static MelonPreferences_Entry<float> SilentNotifHapticAmplitude;

        internal static MelonPreferences_Entry<float> SilentNotifHapticDuration;

        internal static MelonPreferences_Entry<float> NotificationSoundVolume;

        // Shortcuts

        internal static MelonPreferences_Entry<float> ShortcutHoldDuration;

        internal static MelonPreferences_Entry<float> ShortcutInterval;

        // Privacy

        public static MelonPreferences_Entry<bool> ShowPublic;

        public static MelonPreferences_Entry<bool> ShowPrivate;

        public Configuration(MelonPreferences_Category file)
        {
            File = file;

            Orientation = File.CreateEntry("General", WatchHand.Left, "General", "Which hand your watch is placed on (Left / Right)", false, false, null);

            AllowedNotifcationSources = File.CreateEntry("Notification Sources", NotificationSource.All, "Notfication Sources", "The list of sources you have allowed to send their respective notifications", false, false, null);

            NotifHapticAmplitude = File.CreateEntry("Notification Haptic Amplitude", 0.04f, "Notification Haptic Amplitude", "The amplitude of the haptic sent when a notification is recieved", false, false, new ValueRange<float>(0.01f, 0.2f));

            NotifHapticDuration = File.CreateEntry("Notification Haptic Duration", 0.2f, "Notification Haptic Duration", "The duration of the haptic sent when a notification is recieved", false, false, new ValueRange<float>(0.05f, 0.3f));

            SilentNotifHapticAmplitude = File.CreateEntry("Silent Notification Haptic Amplitude", 0.2f, "Silent Notification Haptic Amplitude", "The amplitude of the haptic sent when a silent notification is recieved", false, false, new ValueRange<float>(0.01f, 0.2f));

            SilentNotifHapticDuration = File.CreateEntry("Silent Notification Haptic Duration", 0.1f, "Silent Notification Haptic Duration", "The duration of the haptic sent when a silent notification is recieved", false, false, new ValueRange<float>(0.05f, 0.3f));

            NotificationSoundVolume = File.CreateEntry("Notification Sound Volume", 1f, "Notification Sound Volume", "The volume of the sound produced when a notification is recieved", false, false, new ValueRange<float>(0f, 1f));

            // Use ShortcutHandler class to manage the value of this setting

            ShortcutHoldDuration = File.CreateEntry("Shortcut Hold Duration", 0.25f, "Shortcut Hold Duration", "The maximum duration needed to activate a shortcut using the shortcut button", false, false, new ValueRange<float>(0.25f, 1f));

            ShortcutInterval = File.CreateEntry("Shortcut Inverval", 1f, "Shortcut Inverval", "The minimum interval between shortcut activation using the shortcut button", false, false, new ValueRange<float>(0.5f, 2f));

            ShowPublic = File.CreateEntry("Show Public Name", true, "Show Public Name", "Whether room names under public visibility are shown", false, false, null);

            ShowPrivate = File.CreateEntry("Show Private Name", false, "Show Private Name", "Whether room names under private visibility are shown", false, false, null);
        }

        public enum WatchHand
        {
            Left = 1,
            Right = 0
        }
    }
}
