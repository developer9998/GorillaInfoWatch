using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch.Screens
{
    public class HomeScreen : InfoWatchScreen
    {
        public override string Title => Constants.Name;
        public override string Description => "Created by dev9998 and gizmogoat";

        private Dictionary<string, InfoWatchScreen> entries;

        public void SetEntries(List<InfoWatchScreen> screens)
        {
            Assembly nativeAssembly = Assembly.GetExecutingAssembly();
            var nativeScreens = screens.Where(screen => screen.GetType().Assembly == nativeAssembly).ToList();
            var orderedScreens = nativeScreens.Concat(screens.Except(nativeScreens)).ToList();

            entries = orderedScreens.Where(screen => screen.GetType().GetCustomAttributes(typeof(ShowOnHomeScreenAttribute), false).Any()).ToDictionary(screen => screen.Title, screen => screen);
        }

        public override ScreenContent GetContent()
        {
            var lines = new LineBuilder();

            for (int i = 0; i < entries.Count; i++)
            {
                (string entry_name, InfoWatchScreen screen) = entries.ElementAt(i);
                lines.Add(entry_name, new PushButton(EntrySelected, screen));
            }

            return lines;
        }

        public void EntrySelected(object[] args)
        {
            if (args.ElementAtOrDefault(0) is InfoWatchScreen screen)
            {
                SetScreen(screen.GetType());
            }
        }
    }
}
