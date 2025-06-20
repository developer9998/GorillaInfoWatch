using BepInEx.Configuration;
using GameObjectScheduling;
using GorillaInfoWatch.Extensions;
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

        private readonly string formatSinglePlayer = "<line-height=45%><size=50%>{0}</size><br>";
        private readonly string formatMultiPlayer = "<line-height=45%><size=50%>{0} in {1}</size><br>";

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

                string prepend;

                if (string.IsNullOrEmpty(notification.RoomName)) prepend = string.Format(formatSinglePlayer, timeDisplay);
                else
                {
                    ConfigEntry<bool> roomPrivacySetting = notification.SessionIsPrivate ? Configuration.ShowPrivate : Configuration.ShowPublic;
                    string roomNameProtected = roomPrivacySetting.Value ? notification.RoomName : (notification.SessionIsPrivate ? "private session" : "public session");
                    prepend = string.Format(formatMultiPlayer, timeDisplay, roomNameProtected);
                }

                string[] array = notification.DisplayText.ToTextArray(prepend);

                if (notification.Screen is not null)
                {
                    lines.Add(array, new Widget_PushButton(OpenFunction, notification, true)
                    {
                        Colour = Gradients.Green,
                        Symbol = InfoWatchSymbol.Verified
                    }, new Widget_PushButton(OpenFunction, notification, false)
                    {
                        Colour = Gradients.Red,
                        Symbol = InfoWatchSymbol.Ignore
                    });
                }
                else
                {
                    lines.Add(array, new Widget_PushButton(OpenFunction, notification, true)
                    {
                        Colour = Gradients.Button, // TODO: implement greyscale gradient (white to black)
                        Symbol = InfoWatchSymbol.Invisibility
                    });
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
