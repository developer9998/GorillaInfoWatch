using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch.Screens
{
    public class HomeScreen : WatchScreen
    {
        public override string Title => Constants.Name;

        public override string Description => "Created by dev9998 and lunakitty";

        private List<(string entry_name, WatchScreen screen)> home_entries;

        public void SetEntries(List<WatchScreen> screens)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<WatchScreen> native_screens = [.. screens.Where(screen => screen.GetType().Assembly == assembly)];
            List<WatchScreen> sorted_screens = [.. native_screens.Concat(screens.Except(native_screens))];
            home_entries = new(sorted_screens.Count);
            foreach(WatchScreen screen in sorted_screens)
            {
                if (screen.GetType().GetCustomAttributes(typeof(DisplayAtHomeScreenAttribute), false).FirstOrDefault() is DisplayAtHomeScreenAttribute)
                {
                    home_entries.Add((screen.Title, screen));
                }
            }
        }

        public override void OnScreenOpen()
        {
            LineBuilder = new();

            for (int i = 0; i < home_entries.Count; i++)
            {
                (string entry_name, WatchScreen screen) = home_entries[i];
                LineBuilder.AddLine(entry_name, new WidgetButton(EntrySelected, i));
            }

            SetLines();
        }

        public void EntrySelected(bool value, object[] args)
        {
            if (args[0] is int index)
            {
                ShowScreen(home_entries[index].screen.GetType());
            }
        }
    }
}
