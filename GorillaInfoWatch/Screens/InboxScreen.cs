using BepInEx.Configuration;
using GameObjectScheduling;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    internal class InboxScreen : InfoScreen
    {
        public override string Title => "Inbox";

        public List<Notification> Contents = [];

        private readonly string formatOffline = "<line-height=45%><size=50%>{0}</size><br>", formatOnline = "<line-height=45%><size=50%>{0} in {1}</size><br>";

        public override InfoContent GetContent()
        {
            LineBuilder lines = new();

            if (Contents.Count == 0)
            {
                Description = string.Empty;

                lines.AppendLine("Your inbox is empty.").Skip();
                lines.AppendLine("You're all caught up!");
                lines.AppendLine("You will be alerted of recieved notifications.");

                return lines;
            }

            Description = $"{Contents.Count} notifications recieved";

            foreach (Notification notification in Contents)
            {
                TimeSpan timeSpan = DateTime.Now - notification.Created;
                string timeDisplay = CountdownText.GetTimeDisplay(timeSpan, "{0} {1} ago", int.MaxValue, "Now", string.Empty).ToLower();

                string prepend = string.Format(formatOffline, timeDisplay);

                if (notification.RoomName != null && notification.RoomName.Length > 0)
                {
                    ConfigEntry<bool> roomPrivacySetting = notification.SessionIsPrivate ? Configuration.ShowPrivate : Configuration.ShowPublic;
                    string roomNameProtected = roomPrivacySetting.Value ? notification.RoomName : (notification.SessionIsPrivate ? "private session" : "public session");
                    prepend = string.Format(formatOnline, timeDisplay, roomNameProtected);
                }

                string[] array = notification.DisplayText.ToTextArray(prepend);

                if (notification.Screen is not null)
                {
                    lines.AddRange(array, new Widget_PushButton(OpenFunction, notification, true)
                    {
                        Colour = ColourPalette.Green,
                        Symbol = (Symbol)Symbols.Verified
                    }, new Widget_PushButton(OpenFunction, notification, false)
                    {
                        Colour = ColourPalette.Red,
                        Symbol = (Symbol)Symbols.Ignore
                    });
                    continue;
                }

                lines.AddRange(array, new Widget_PushButton(OpenFunction, notification, true)
                {
                    Colour = ColourPalette.Black,
                    Symbol = (Symbol)Symbols.Invisibility
                });
            }

            return lines;
        }

        private void OpenFunction(object[] args)
        {
            Logging.Info(string.Join(", ", args.Select(arg => arg.GetType().Name)));
            if (args.ElementAtOrDefault(0) is Notification notification && args.ElementAtOrDefault(1) is bool digest) OpenNotification(notification, digest);
        }

        private void OpenNotification(Notification notification, bool digest)
        {
            Logging.Info($"OpenNotification \"{notification.DisplayText}\"");
            Notifications.OpenNotification(notification, digest);
        }
    }
}
