using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
using System;
using System.Reflection;
using static RotatingQuestsManager;

namespace GorillaInfoWatch
{
    public class Events
    {
        internal static Action<VRRig> OnUpdateName;
        internal static Action<VRRig> OnGetUserCosmetics;
        internal static Action<VRRig, bool> OnSetInvisibleToLocalPlayer;

        internal static Action<RotatingQuest> OnQuestCompleted;

        internal static Action<GorillaGameManager, NetPlayer, NetPlayer> OnPlayerTagged;
        internal static Action<GorillaGameManager> OnRoundComplete;

        // GorillaInfoWatch

        internal static Action<Notification> OnNotificationSent;
        internal static Action<Notification, bool> OnNotificationOpened;

        internal static Action<NetPlayer, PlayerSignificance> OnSignificanceChanged;

        public static void SendNotification(Notification notification)
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));

            Assembly assembly = Assembly.GetCallingAssembly();
            Logging.Message($"Notification sending from {assembly.GetName().Name}:\n{notification.DisplayText}");

            OnNotificationSent?.SafeInvoke(notification);
        }

        public static void OpenNotification(Notification notification, bool digest)
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));

            OnNotificationOpened?.SafeInvoke(notification, digest);
        }
    }
}