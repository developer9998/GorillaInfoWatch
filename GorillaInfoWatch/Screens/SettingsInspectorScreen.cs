using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Configuration;

namespace GorillaInfoWatch.Screens;

internal class SettingsInspectorScreen : InfoScreen
{
    public override string Title => Section.Title;

    public override string Description => $"Section contains {Section.Entries.Count} entries";

    public static ConfigurableSection Section;

    public override InfoContent GetContent()
    {
        PageBuilder pages = new();

        foreach (ConfigurableWrapper entry in Section.Entries)
        {
            LineBuilder lines = new();
            entry.WriteLines(lines);
            pages.Add(lines);
        }

        return pages;
    }
}
