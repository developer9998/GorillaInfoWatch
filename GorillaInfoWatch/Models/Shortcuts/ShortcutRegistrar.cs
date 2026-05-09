using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using static GorillaInfoWatch.Models.Notification;

namespace GorillaInfoWatch.Models.Shortcuts;

public abstract class ShortcutRegistrar
{
    public abstract string Title { get; }

    public ReadOnlyCollection<Shortcut> Shortcuts => _shortcuts.AsReadOnly();

    public static ReadOnlyCollection<Shortcut> AllShortcuts => _allShortcuts.AsReadOnly();

    private readonly List<Shortcut> _shortcuts = [];

    private static readonly List<Shortcut> _allShortcuts = [];

    public void RegisterShortcut(string name, string description, Action method) => RegisterShortcut(name, description, ShortcutRestrictions.None, method, Assembly.GetCallingAssembly());

    public void RegisterShortcut(string name, string description, ShortcutRestrictions restrictions, Action method) => RegisterShortcut(name, description, restrictions, method, Assembly.GetCallingAssembly());

    private void RegisterShortcut(string name, string description, ShortcutRestrictions restrictions, Action method, Assembly assembly)
    {
        Shortcut shortcut = new(assembly, name, description, restrictions, method);
        _shortcuts.Add(shortcut);
        _allShortcuts.Add(shortcut);
    }

    internal void Notify(Notification notification) => Notifications.SendNotification(notification);

    public void Notify(string content, float duration, Sounds sound = Sounds.None, ExternalScreen screen = null) => Notify(new(content, duration, sound, screen));

    public void Notify(string head, string body, float duration, Sounds sound = Sounds.None, ExternalScreen screen = null) => Notify(new(head, body, duration, sound, screen));
}