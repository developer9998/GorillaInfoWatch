﻿using GorillaInfoWatch.Attributes;
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

        private readonly Dictionary<string, InfoWatchScreen> entries = [];
        private readonly LineBuilder lineBuilder = new();

        public void SetEntries(List<InfoWatchScreen> screens)
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

        public override ScreenContent GetContent()
        {
            lineBuilder.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                (string title, InfoWatchScreen screen) = entries.ElementAt(i);
                lineBuilder.Add(title, new Widget_PushButton(SelectScreen, screen));
            }

            return lineBuilder;
        }

        public void SelectScreen(object[] args)
        {
            if (args.ElementAtOrDefault(0) is InfoWatchScreen screen)
            {
                SetScreen(screen.GetType());
            }
        }
    }
}
