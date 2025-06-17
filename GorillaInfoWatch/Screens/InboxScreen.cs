using GameObjectScheduling;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    internal class InboxScreen : InfoWatchScreen
    {
        public override string Title => "Inbox";

        public List<Notification> Notifications = [];

        private readonly string formatMultiPlayer = "<line-height=45%><size=50%>{0} in {1}</size><br>{2}";

        private readonly string formatSinglePlayer = "<line-height=45%><size=50%>{0}</size><br>{1}";

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
                TimeSpan timeSpan = DateTime.Now - notification.Created;
                string timeDisplay = CountdownText.GetTimeDisplay(timeSpan, "{0} {1} ago").ToLower();
                string content = string.Format(formatSinglePlayer, timeDisplay, notification.DisplayText);
                if (!string.IsNullOrEmpty(notification.RoomName))
                {
                    string roomName = notification.SessionIsPrivate ? "private session" : notification.RoomName;
                    content = string.Format(formatMultiPlayer, timeDisplay, roomName, notification.DisplayText);
                }

                if (notification.Screen is not null)
                {
                    lines.Add(content, new Widget_PushButton(OpenFunction, notification, true)
                    {
                        Colour = Gradients.Green
                    }, new Widget_PushButton(OpenFunction, notification, false)
                    {
                        Colour = Gradients.Red
                    });
                }
                else
                {
                    lines.Add(content, new Widget_PushButton(OpenFunction, notification, true));
                }
            }

            return lines;
        }

        public void OpenFunction(object[] args)
        {
            Logging.Info(string.Join(", ", args.Select(arg => arg.GetType().Name)));
            if (args.ElementAtOrDefault(0) is Notification notification && args.ElementAtOrDefault(1) is bool digest)
                OpenNotification(notification, digest);
        }

        public void OpenNotification(Notification notification, bool digest)
        {
            Logging.Info($"OpenNotification \"{notification.DisplayText}\"");
            Events.OpenNotification(notification, digest);
        }
    }
}
