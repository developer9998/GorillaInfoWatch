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

        internal List<Shortcut> _entries;


        internal void SetEntries(IList<Shortcut> list)
        {
            Assembly nativeAssembly = Assembly.GetExecutingAssembly();

            IEnumerable<Shortcut> nativeScreens = list.Where(screen => screen.GetType().Assembly == nativeAssembly);
            _entries = [.. nativeScreens, .. list.Except(nativeScreens)];
        }

        public override InfoContent GetContent()
        {
            LineBuilder lines = new();

            foreach (Shortcut shortcut in _entries)
            {
                string shortcutText = string.Format("<line-height=45%>{0}<br><size=60%>{1}", shortcut.Name, shortcut.Description);
                lines.Add(shortcut.Name, new Widget_Switch(ShortcutManager.Instance.Shortcut == shortcut, (bool value) =>
                {
                    ShortcutManager.Instance.SetOrRemoveShortcut(shortcut);
                    SetContent();
                }));
            }

            return lines;
        }
    }
}
