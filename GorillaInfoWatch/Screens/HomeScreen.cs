using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch.Screens
{
    public class HomeScreen : InfoScreen
    {
        public override string Title => Constants.Name;
        public override string Description => "Created by Dev [dev9998] and Gizmo [gizmogoat]";

        internal readonly Dictionary<string, InfoScreen> entries = [];
        internal readonly LineBuilder lineBuilder = new();

        internal void SetEntries(List<InfoScreen> screens)
        {
            Assembly nativeAssembly = typeof(Plugin).Assembly;

            var nativeScreens = screens.Where(screen => screen.GetType().Assembly == nativeAssembly);
            var orderedScreens = nativeScreens.Concat(screens.Except(nativeScreens)).ToList();

            if (entries.Count > 0) entries.Clear();
            foreach (var screen in orderedScreens)
            {
                if (screen.GetType().GetCustomAttribute<ShowOnHomeScreenAttribute>() is ShowOnHomeScreenAttribute attribute && attribute != null)
                {
                    string title = (string.IsNullOrEmpty(attribute.DisplayTitle) || string.IsNullOrWhiteSpace(attribute.DisplayTitle)) ? screen.Title : attribute.DisplayTitle;
                    entries.Add(title, screen);
                }
            }
        }

        public override InfoContent GetContent()
        {
            lineBuilder.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                (string title, InfoScreen screen) = entries.ElementAt(i);
                lineBuilder.Add(title, new Widget_PushButton(SelectScreen, screen));
            }

            return lineBuilder;
        }

        internal void SelectScreen(object[] args)
        {
            if (args.ElementAtOrDefault(0) is InfoScreen screen)
            {
                LoadScreen(screen.GetType());
            }
        }
    }
}
