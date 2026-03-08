using GorillaInfoWatch.Models;
using GorillaLibrary;
using MelonLoader;
using MelonLoader.Preferences;

namespace GorillaInfoWatch.Tools
{
    public class Configuration
    {
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

        public Configuration(GorillaMod gorillaMod)
        {
            MelonPreferences_Category categoryGeneral = gorillaMod.CreateCategory("General");
            Orientation = categoryGeneral.CreateEntry("Orientation", WatchHand.Left, "Orientation", "Which hand your watch is placed on (Left / Right)", false, false, null);

            MelonPreferences_Category categoryNotifications = gorillaMod.CreateCategory("Notifications");
            AllowedNotifcationSources = categoryNotifications.CreateEntry("Notification Sources", NotificationSource.All, "Notfication Sources", "The list of sources you have allowed to send their respective notifications", false, false, null);
            NotifHapticAmplitude = categoryNotifications.CreateEntry("Notification Haptic Amplitude", 0.04f, "Notification Haptic Amplitude", "The amplitude of the haptic sent when a notification is recieved", false, false, new ValueRange<float>(0.01f, 0.2f));
            NotifHapticDuration = categoryNotifications.CreateEntry("Notification Haptic Duration", 0.2f, "Notification Haptic Duration", "The duration of the haptic sent when a notification is recieved", false, false, new ValueRange<float>(0.05f, 0.3f));
            SilentNotifHapticAmplitude = categoryNotifications.CreateEntry("Silent Notification Haptic Amplitude", 0.2f, "Silent Notification Haptic Amplitude", "The amplitude of the haptic sent when a silent notification is recieved", false, false, new ValueRange<float>(0.01f, 0.2f));
            SilentNotifHapticDuration = categoryNotifications.CreateEntry("Silent Notification Haptic Duration", 0.1f, "Silent Notification Haptic Duration", "The duration of the haptic sent when a silent notification is recieved", false, false, new ValueRange<float>(0.05f, 0.3f));
            NotificationSoundVolume = categoryNotifications.CreateEntry("Notification Sound Volume", 1f, "Notification Sound Volume", "The volume of the sound produced when a notification is recieved", false, false, new ValueRange<float>(0f, 1f));

            // Use ShortcutHandler class to manage the value of this setting
            MelonPreferences_Category categoryInteractions = gorillaMod.CreateCategory("Interactions");
            ShortcutHoldDuration = categoryInteractions.CreateEntry("Shortcut Hold Duration", 0.25f, "Shortcut Hold Duration", "The maximum duration needed to activate a shortcut using the shortcut button", false, false, new ValueRange<float>(0.25f, 1f));
            ShortcutInterval = categoryInteractions.CreateEntry("Shortcut Inverval", 1f, "Shortcut Inverval", "The minimum interval between shortcut activation using the shortcut button", false, false, new ValueRange<float>(0.5f, 2f));

            MelonPreferences_Category categoryPrivacy = gorillaMod.CreateCategory("Privacy");
            ShowPublic = categoryPrivacy.CreateEntry("Show Public Name", true, "Show Public Name", "Whether room names under public visibility are shown", false, false, null);
            ShowPrivate = categoryPrivacy.CreateEntry("Show Private Name", false, "Show Private Name", "Whether room names under private visibility are shown", false, false, null);
        }

        public enum WatchHand
        {
            Left = 1,
            Right = 0
        }
    }
}
