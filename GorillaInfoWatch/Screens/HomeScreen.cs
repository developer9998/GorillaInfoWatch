using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;

namespace GorillaInfoWatch.Screens
{
    public class HomeScreen : WatchScreen
    {
        public override string Title => Constants.Name;

        public override string Description => "Created by dev9998 and gizmogoat";

        private List<(string entry_name, WatchScreen screen)> home_entries;

        public void SetEntries(List<WatchScreen> screens)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<WatchScreen> native_screens = [.. screens.Where(screen => screen.GetType().Assembly == assembly)];
            List<WatchScreen> sorted_screens = [.. native_screens.Concat(screens.Except(native_screens))];
            home_entries = new(sorted_screens.Count);
            foreach (WatchScreen screen in sorted_screens)
            {
                if (screen.GetType().GetCustomAttributes(typeof(DisplayAtHomeScreenAttribute), false).FirstOrDefault() is DisplayAtHomeScreenAttribute)
                {
                    home_entries.Add((screen.Title, screen));
                }
            }
        }

        public override ScreenContent GetContent()
        {
            var lines = new LineBuilder();

            for (int i = 0; i < home_entries.Count; i++)
            {
                (string entry_name, WatchScreen screen) = home_entries[i];
                lines.AddLine(entry_name, new WidgetButton(EntrySelected, i));
            }

            return lines;
        }

        public void EntrySelected(bool value, object[] args)
        {
            if (args[0] is int index)
            {
                SetScreen(home_entries[index].screen.GetType());
            }
        }
    }
}
