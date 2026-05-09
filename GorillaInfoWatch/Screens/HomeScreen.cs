using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch.Screens;

[PreserveSection]
public class HomeScreen : InfoScreen
{
    public override string Title => Constants.Name;
    public override string Description => "Created by Dev / dev9998 and Gizmo / gizmogoat";

    internal readonly List<(string title, InfoScreen screen, bool native)> entries = [];

    internal void SetEntries(List<InfoScreen> screens)
    {
        Assembly nativeAssembly = Assembly.GetExecutingAssembly();

        var nativeScreens = screens.Where(screen => screen.GetType().Assembly == nativeAssembly);
        var orderedScreens = nativeScreens.Concat(screens.Except(nativeScreens)).ToList();

        foreach (var screen in orderedScreens)
        {
            if (screen.GetType().GetCustomAttribute<ShowOnHomeScreenAttribute>() is ShowOnHomeScreenAttribute attribute && attribute != null)
            {
                string title = (string.IsNullOrEmpty(attribute.DisplayTitle) || string.IsNullOrWhiteSpace(attribute.DisplayTitle)) ? screen.Title : attribute.DisplayTitle;
                entries.Add((title, screen, nativeScreens.Contains(screen)));
            }
        }
    }

    public override InfoContent GetContent()
    {
        LineBuilder internalLines = new(), externalLines = new();

        for (int i = 0; i < entries.Count; i++)
        {
            (string screenTitle, InfoScreen screenInstance, bool internalScreen) = entries[i];
            LineBuilder lines = internalScreen ? internalLines : externalLines;
            lines.Add(screenTitle, new Widget_PushButton(SelectScreen, screenInstance));
        }

        if (externalLines.Lines.Count > 0)
        {
            PageBuilder pages = new();
            pages.Add(internalLines);
            pages.Add(externalLines);
            return pages;
        }

        return internalLines;
    }

    internal void SelectScreen(object[] args)
    {
        if (args.ElementAtOrDefault(0) is InfoScreen screen)
        {
            LoadScreen(screen.GetType());
        }
    }
}
