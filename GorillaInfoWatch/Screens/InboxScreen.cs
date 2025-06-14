using GorillaInfoWatch.Models;
using System.Collections.Generic;
using System.Linq;
using GorillaInfoWatch.Models.Widgets;

namespace GorillaInfoWatch.Screens
{
    internal class InboxScreen : Screen
    {
        public override string Title => "Inbox";

        public List<Notification> Inbox = [];

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            if (Inbox.Count == 0)
            {
                Description = string.Empty;

                lines.AddLine("No unopened notifications.");
                lines.AddLine();
                lines.AddLine("You're all caught up!");

                return lines;
            }

            Description = $"{Inbox.Count} unopened notifications";

            Inbox.ForEach(notification => lines.AddLine(notification.DisplayText, new PushButton(OpenNotification, notification)));

            return lines;
        }

        public void OpenNotification(object[] args)
        {
            if (args.ElementAtOrDefault(0) is Notification notification)
                OpenNotification(notification);
        }

        public void OpenNotification(Notification notification)
        {
            if (Inbox.Contains(notification))
                Events.OpenNotification(notification);
        }
    }
}
