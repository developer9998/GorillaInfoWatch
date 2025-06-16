using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    internal class InboxScreen : InfoWatchScreen
    {
        public override string Title => "Inbox";

        public List<Notification> Notifications = [];

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            if (Notifications.Count == 0)
            {
                Description = string.Empty;

                lines.Add("<align=\"center\">Inbox is empty - no new notifications.</align>");
                lines.Skip();
                lines.Add("<align=\"center\">You're all caught up!</align>");

                return lines;
            }

            Description = $"{Notifications.Count} new notifications";

            foreach (var notification in Notifications)
            {
                string content = string.Format("<line-height=45%><size=3.5>{0} in {1}</size><br>{2}", notification.Created.ToLongTimeString(), notification.SessionIsPrivate ? "private room" : notification.RoomName, notification.DisplayText);

                if (notification.Screen is not null)
                {
                    lines.Add(content, new PushButton(OpenNotification, notification, true)
                    {
                        Colour = Gradients.Green
                    }, new PushButton(OpenNotification, notification, false)
                    {
                        Colour = Gradients.Red
                    });
                }
                else
                {
                    lines.Add(content, new PushButton(OpenNotification, notification, true));
                }
            }

            return lines;
        }

        public void OpenNotification(object[] args)
        {
            if (args.ElementAtOrDefault(0) is Notification notification && args.ElementAtOrDefault(1) is bool digest)
                OpenNotification(notification, digest);
        }

        public void OpenNotification(Notification notification, bool digest)
        {
            if (Notifications.Contains(notification))
                Events.OpenNotification(notification, digest);
        }
    }
}
