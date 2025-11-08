using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen, PreserveScreenSection]
    internal class ShortcutListScreen : InfoScreen
    {
        public override string Title => "Shortcuts";
        public override string Description => "Choose a bind for the shortcut button and inspect shortcuts";

        internal List<ShortcutRegistrar> _entries;

        internal void SetEntries(IList<ShortcutRegistrar> list)
        {
            Assembly nativeAssembly = Assembly.GetExecutingAssembly();

            IEnumerable<ShortcutRegistrar> nativeScreens = list.Where(screen => screen.GetType().Assembly == nativeAssembly);
            _entries = [.. nativeScreens, .. list.Except(nativeScreens)];
        }

        public override InfoContent GetContent()
        {
            PageBuilder pages = new();

            foreach (ShortcutRegistrar registrar in _entries)
            {
                LineBuilder lines = new();

                foreach (Shortcut shortcut in registrar.Shortcuts)
                {
                    string shortcutText = string.Format("<line-height=45%>{0}<br><size=60%>{1}", shortcut.Name, shortcut.Description);
                    lines.Add(shortcutText, new Widget_PushButton(() =>
                    {
                        ShortcutHandler.Instance.ExcecuteShortcut(shortcut);
                    })
                    {
                        Colour = ColourPalette.Green,
                        Symbol = Symbol.GetSharedSymbol(Symbols.Play)
                    }, new Widget_Switch(ShortcutHandler.Instance.Shortcut == shortcut, (bool value) =>
                    {
                        ShortcutHandler.Instance.SetOrRemoveShortcut(shortcut);
                        SetContent();
                    }));
                }

                pages.Add(lines, registrar.Title);
            }

            return pages;
        }
    }
}
