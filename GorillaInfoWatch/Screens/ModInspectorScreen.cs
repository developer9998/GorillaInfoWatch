using BepInEx;
using BepInEx.Configuration;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Configuration;
using GorillaInfoWatch.Models.Widgets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    internal class ModInspectorScreen : InfoScreen
    {
        public override string Title => "Mod Inspector";
        public override Type ReturnType => typeof(ModListScreen);

        public static PluginInfo Mod;

        private List<ConfigurableSection> _configurableList = null;

        public override void OnScreenLoad()
        {
            if (Mod.Instance.Config is not ConfigFile file || file.Count == 0)
                return;

            var entries = file.GetEntries();
            var sectionNames = entries.Select(entry => entry.Definition.Section).Distinct();
            var dictionary = sectionNames.ToDictionary(section => section, section => new ConfigurableSection()
            {
                Title = section,
                Entries = []
            });
            entries.ForEach(entry => dictionary[entry.Definition.Section].Entries.Add(new ConfigurableWrapper_BepInEntry(entry)));
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

            lines.Add($"Name: {Mod.Metadata.Name}");
            lines.Add($"Version: {Mod.Metadata.Version}");
            lines.Add($"GUID: {Mod.Metadata.GUID}");

            lines.Skip();

            lines.Add($"State: {(Mod.Instance.enabled ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}", new Widget_Switch(Mod.Instance.enabled, ToggleMod, Mod));

            var methods = AccessTools.GetMethodNames(Mod.Instance);
            if (!methods.Contains("OnEnable") && !methods.Contains("OnDisable"))
                lines.Add("<color=red>This mod may not support the behaviour state!");

            if (_configurableList == null) return lines;

            PageBuilder pages = new();
            pages.Add(Mod.Metadata.Name, lines);

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
            if (parameters.ElementAtOrDefault(0) is PluginInfo pluginInfo)
            {
                pluginInfo.Instance.enabled = value;
                Main.Instance.SetPersistentPluginState(pluginInfo, value);

                SetText();
            }
        }
    }
}
