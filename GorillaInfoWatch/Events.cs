using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using System;
using static RotatingQuestsManager;

namespace GorillaInfoWatch
{
    public class Events
    {
        // GT

        internal static Action<VRRig> OnGetUserCosmetics;

        internal static Action<VRRig, bool> OnSetInvisibleToLocalPlayer;

        internal static Action<RotatingQuest> OnCompleteQuest;

        // GorillaInfoWatch

        internal static Action<Notification> OnNotificationSent;

        internal static Action<Notification, bool> OnNotificationOpened;

        public static Action<NetPlayer, PlayerSignificance> OnSignificanceChanged;

        public static void SendNotification(Notification notification)
        {
            if (notification is null)
                throw new ArgumentNullException(nameof(notification));

            OnNotificationSent?.SafeInvoke(notification);
        }

        public static void OpenNotification(Notification notification, bool digest)
        {
            if (notification is null)
                throw new ArgumentNullException(nameof(notification));

            OnNotificationOpened?.SafeInvoke(notification, digest);
        }
    }
}