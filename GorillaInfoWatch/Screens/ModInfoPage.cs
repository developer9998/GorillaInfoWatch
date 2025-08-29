using BepInEx;
using BepInEx.Configuration;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    internal class ModInfoPage : InfoScreen
    {
        public override string Title => "Mod Inspector";
        public override Type ReturnType => typeof(ModListPage);

        public static PluginInfo Mod;

        public override InfoContent GetContent()
        {
            if (Mod is null)
            {
                LoadScreen<ModListPage>();
                return null;
            }

            LineBuilder lines = new();

            lines.Add($"Name: {Mod.Metadata.Name}");
            lines.Add($"Version: v{Mod.Metadata.Version}");
            lines.Add($"GUID: {Mod.Metadata.GUID}");

            lines.Skip();

            lines.Add($"State: {(Mod.Instance.enabled ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}", new Widget_Switch(Mod.Instance.enabled, ToggleMod, Mod));

            var methods = AccessTools.GetMethodNames(Mod.Instance);
            if (!methods.Contains("OnEnable") && !methods.Contains("OnDisable"))
                lines.Add("<color=red>This mod may not support the behaviour state!");

            if (Mod.Instance.Config is null || Mod.Instance.Config.Count == 0)
                return lines;

            PageBuilder pages = new();

            pages.AddPage(Mod.Metadata.Name, lines);

            var entries = Mod.Instance.Config.Keys.Select(key => Mod.Instance.Config[key]);

            foreach (var configEntry in entries)
            {
                Type settingType = configEntry.SettingType;

                if (!configEntry.SettingType.IsEnum && configEntry.SettingType != typeof(int) && configEntry.SettingType != typeof(bool))
                    continue;

                lines = new();

                List<Widget_Base> widgets = [];

                if (configEntry.SettingType.IsEnum)
                {
                    var names = Enum.GetNames(settingType);
                    widgets.Add(new Widget_SnapSlider(Array.IndexOf(names, configEntry.GetSerializedValue()) is int value && value != -1 ? value : 0, 0, names.Length - 1, ConfigureEntry, configEntry));
                }
                else if (configEntry.SettingType == typeof(int))
                {
                    if (configEntry.Description.AcceptableValues is AcceptableValueRange<int> range)
                        widgets.Add(new Widget_SnapSlider(int.Parse(configEntry.BoxedValue.ToString(), CultureInfo.InvariantCulture), range.MinValue, range.MaxValue, ConfigureEntry, configEntry));
                    else
                    {
                        // TODO: add +/- buttons for adjusting integer
                    }
                }
                else if (configEntry.SettingType == typeof(bool))
                    widgets.Add(new Widget_Switch(configEntry.BoxedValue.ToString() == "True", ConfigureEntry, configEntry));

                lines.Add($"Key: {configEntry.Definition.Key}");
                lines.AddRange($"Description: {configEntry.Description.Description}".ToTextArray());
                lines.Add($"Type: {settingType.Name}");

                lines.Skip();

                lines.Add($"Value: {configEntry.GetSerializedValue()}", widgets);

                pages.AddPage(configEntry.Definition.Section, lines);
            }

            return pages;
        }

        public void ToggleMod(bool value, object[] parameters)
        {
            if (parameters.ElementAtOrDefault(0) is PluginInfo info)
            {
                info.Instance.enabled = value;
                SetText();
            }
        }

        public void ConfigureEntry(bool value, object[] parameters)
        {
            if (parameters.ElementAtOrDefault(0) is ConfigEntryBase entry && entry.SettingType == typeof(bool))
            {
                entry.SetSerializedValue(value.ToString());
                SetText();
            }
        }

        public void ConfigureEntry(int value, object[] parameters)
        {
            if (parameters.ElementAtOrDefault(0) is ConfigEntryBase entry)
            {
                if (entry.SettingType == typeof(int))
                {
                    entry.SetSerializedValue(value.ToString());
                    SetText();
                }
                else if (entry.SettingType.IsEnum && Enum.GetNames(entry.SettingType).ElementAtOrDefault(value) is string enumName)
                {
                    entry.SetSerializedValue(enumName);
                    SetText();
                }
            }
        }
    }
}
