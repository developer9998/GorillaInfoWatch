using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using System;
using static RotatingQuestsManager;

namespace GorillaInfoWatch
{
    public class Events
    {
        // GT

        public static Action<VRRig> OnGetUserCosmetics;

        public static Action<VRRig, bool> OnSetInvisibleToLocalPlayer;

        public static Action<RotatingQuest> OnCompleteQuest;

        // GorillaInfoWatch

        public static Action<Notification> OnNotificationSent;

        public static Action<Notification, bool> OnNotificationOpened;

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