using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Shortcuts;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen, PreserveSection]
    internal class ShortcutListScreen : InfoScreen
    {
        public override string Title => "Shortcuts";
        public override string Description => "Choose a bind for the shortcut button and inspect shortcuts";

        internal List<ShortcutCategory> _entries;

        public override void OnScreenLoad()
        {
            if (_entries == null)
            {
                Assembly nativeAssembly = Assembly.GetExecutingAssembly();
                var list = ShortcutHandler.Instance.Categories;
                IEnumerable<ShortcutCategory> nativeCategories = list.Where(category => category.assembly == nativeAssembly);
                _entries = [.. nativeCategories, .. list.Except(nativeCategories)];
            }
        }

        public override InfoContent GetContent()
        {
            PageBuilder pages = new();

            foreach (ShortcutCategory category in _entries)
            {
                LineBuilder lines = new();

                foreach (Shortcut shortcut in category.shortcuts)
                {
                    string shortcutText = string.Format("<line-height=45%>{0}<br><size=60%>{1}", shortcut.Name, shortcut.Description);
                    lines.Add(shortcutText, new Widget_PushButton(() =>
                    {
                        ShortcutHandler.Instance.ExcecuteShortcut(shortcut);
                    })
                    {
                        Alignment = WidgetAlignment.Left,
                        Colour = ColourPalette.Green,
                        Symbol = Content.Shared.Symbols["Play"]
                    }, new Widget_Switch(ShortcutHandler.Shortcut == shortcut, (bool value) =>
                    {
                        ShortcutHandler.Instance.SetOrRemoveShortcut(shortcut);
                        SetContent();
                    }));
                }

                pages.Add(category.Title, lines);
            }

            return pages;
        }
    }
}