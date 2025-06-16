using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    internal class InboxScreen : InfoWatchScreen
    {
        public override string Title => "Inbox";

        public List<Notification> Inbox = [];

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            if (Inbox.Count == 0)
            {
                Description = string.Empty;

                lines.Add("<align=\"center\">Inbox is empty - no new notifications.</align>");
                lines.Skip();
                lines.Add("<align=\"center\">You're all caught up!</align>");

                return lines;
            }

            Description = $"{Inbox.Count} new notifications";

            foreach (var notification in Inbox)
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
            if (Inbox.Contains(notification))
                Events.OpenNotification(notification, digest);
        }
    }
}
