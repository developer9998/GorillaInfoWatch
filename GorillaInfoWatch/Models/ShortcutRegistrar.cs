using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace GorillaInfoWatch.Models
{
    public abstract class ShortcutRegistrar
    {
        public abstract string Title { get; }

        public static ReadOnlyCollection<Shortcut> Shortcuts => _shortcuts.AsReadOnly();

        private static readonly List<Shortcut> _shortcuts = [];

        public void RegisterShortcut(string name, string description, Action method)
        {
            Assembly source = Assembly.GetCallingAssembly();
            Shortcut function = new(source, name, description, method);
            _shortcuts.Add(function);
        }
    }
}
