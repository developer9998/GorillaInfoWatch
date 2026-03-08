using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Configuration;
using GorillaInfoWatch.Models.Widgets;
using GorillaLibrary;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    internal class ModInspectorScreen : InfoScreen
    {
        public override string Title => "Mod Inspector";
        public override Type ReturnType => typeof(ModListScreen);

        public static MelonMod Mod;

        private List<ConfigurableSection> _configurableList = null;

        public override void OnScreenLoad()
        {
            if (Mod is not GorillaMod gm || gm.Categories.Sum(category => category.Entries.Count) == 0) return;

            var entries = gm.Categories.SelectMany(category => category.Entries);
            var sectionNames = entries.Select(entry => entry.Category.DisplayName).Distinct();
            var dictionary = sectionNames.ToDictionary(section => section, section => new ConfigurableSection()
            {
                Title = section,
                Entries = []
            });
            entries.ForEach(entry => dictionary[entry.Category.DisplayName].Entries.Add(new ConfigurableWrapper_ML(entry)));
            _configurableList = [.. dictionary.Values];
        }

        public override void OnScreenUnload()
        {
            _configurableList = null;
        }

        public override InfoContent GetContent()
        {
            if (Mod is null)
            {
                LoadScreen<ModListScreen>();
                return null;
            }

            LineBuilder lines = new();

            lines.Add($"Name: {Mod.Info.Name}");
            lines.Add($"Version: {Mod.Info.Version}");

            if (Mod is GorillaMod gm)
            {
                lines.Skip().Add($"State: {(gm.Enabled ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}", new Widget_Switch(gm.Enabled, ToggleMod, Mod));
                if (!gm.IsStateSupported) lines.Add("<color=red>This mod does not implement enable states");
            }

            if (_configurableList == null) return lines;

            PageBuilder pages = new();
            pages.Add(Mod.Info.Name, lines);

            foreach (ConfigurableSection section in _configurableList)
            {
                foreach (ConfigurableWrapper wrapper in section.Entries)
                {
                    lines = new();
                    wrapper.WriteLines(lines);
                    pages.Add(section.Title, lines);
                }
            }

            return pages;
        }

        public void ToggleMod(bool value, object[] parameters)
        {
            if (Mod is GorillaMod gm)
            {
                gm.Enabled = value;

                SetText();
            }
        }
    }
}
