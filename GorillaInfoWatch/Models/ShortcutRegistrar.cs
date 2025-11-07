using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using static GorillaInfoWatch.Models.Notification;

namespace GorillaInfoWatch.Models
{
    public abstract class ShortcutRegistrar
    {
        public static ReadOnlyCollection<Shortcut> Shortcuts => _shortcuts.AsReadOnly();

        public abstract string Title { get; }

        private static readonly List<Shortcut> _shortcuts = [];

        public void RegisterShortcut(string name, string description, Action method)
        {
            Assembly source = Assembly.GetCallingAssembly();
            Shortcut function = new(source, name, description, method);
            _shortcuts.Add(function);
        }

        internal void Notify(Notification notification) => Notifications.SendNotification(notification);

        public void Notify(string content, float duration, Sounds sound = Sounds.None, ExternalScreen screen = null) => Notify(new(content, duration, sound, screen));

        public void Notify(string head, string body, float duration, Sounds sound = Sounds.None, ExternalScreen screen = null) => Notify(new(head, body, duration, sound, screen));
    }
}
