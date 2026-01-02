using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Configuration;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorillaInfoWatch.Screens;

[ShowOnHomeScreen]
internal class SettingsListScreen : InfoScreen
{
    public override string Title => "Settings";
    public override string Description => "Configure your current installation of GorillaInfoWatch. Your changes will be restored when you open Gorilla Tag.";

    private List<ConfigurableSection> _configurableList;

    public void Awake()
    {
        var entries = Plugin.Config.File.GetEntries();
        var sectionNames = entries.Select(entry => entry.Definition.Section).Distinct();
        var dictionary = sectionNames.ToDictionary(section => section, section => new ConfigurableSection()
        {
            Title = section,
            Entries = []
        });

        StringBuilder str = new();
        str.AppendLine("The permissions you have set for viewing significance of your account, measured with:");
        str.AppendLine("Item: allocated entirely for users with distinctive cosmetics, like creator badges and sticks");
        str.AppendLine("Figure: allocated usually for users that helped with the mod, as shown on the credits screen");
        dictionary["Privacy"].Entries.Insert(0, new ConfigurableWrapper_Woaw<SignificanceVisibility>("Significance Visibility", str.ToString(), () => SignificanceManager.Instance.Visibility, consent => SignificanceManager.Instance.SetVisibility(consent)));

        entries.ForEach(entry => dictionary[entry.Definition.Section].Entries.Add(new ConfigurableWrapper_BepInEntry(entry)));
        _configurableList = [.. dictionary.Values];
    }

    public override InfoContent GetContent()
    {
        LineBuilder lines = new();

        foreach (ConfigurableSection section in _configurableList)
        {
            lines.Add(section.Title, new Widget_PushButton(OpenSection, section)
            {
                Colour = ColourPalette.Black,
                Symbol = Symbol.GetSharedSymbol(Symbols.ExternalLink)
            });
        }

        return lines;
    }

    public void OpenSection(object[] parameters)
    {
        ConfigurableSection section = (ConfigurableSection)parameters[0];
        SettingsInspectorScreen.Section = section;
        LoadScreen<SettingsInspectorScreen>();
    }
}
