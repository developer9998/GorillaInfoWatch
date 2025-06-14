using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using System;
using static RotatingQuestsManager;

namespace GorillaInfoWatch
{
    public class Events
    {
        // GT

        public static Action<VRRig, bool> OnSetInvisibleToLocalPlayer;

        public static Action<RotatingQuest> OnCompleteQuest;

        // GorillaInfoWatch

        public static Action<Notification> OnNotificationSent, OnNotificationOpened;

        public static void SendNotification(Notification notification)
        {
            if (notification is null)
                throw new ArgumentNullException(nameof(notification));

            OnNotificationSent?.SafeInvoke(notification);
        }

        public static void OpenNotification(Notification notification)
        {
            if (notification is null)
                throw new ArgumentNullException(nameof(notification));

            OnNotificationOpened?.SafeInvoke(notification);
        }
    }
}