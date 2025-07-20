using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class ModListPage : InfoWatchScreen
    {
        public override string Title => "Mods";

        private PluginInfo[] stateSupportedMods, mods;

        public void Awake()
        {
            Dictionary<string, PluginInfo> _loadedPlugins = Chainloader.PluginInfos;
            PluginInfo[] _pluginInfos = [.. _loadedPlugins.Values];

            stateSupportedMods = [.. _pluginInfos.Where(delegate(PluginInfo pluginInfo)
            {
                List<string> _instanceMethods = AccessTools.GetMethodNames(pluginInfo.Instance);
                return _instanceMethods.Contains("OnEnable") || _instanceMethods.Contains("OnDisable");
            }).OrderBy(pluginInfo => pluginInfo.Metadata.Name)];

            mods = [.. stateSupportedMods.Concat(_pluginInfos.Except(stateSupportedMods).OrderBy(pluginInfo => pluginInfo.Metadata.Name).OrderByDescending(pluginInfo => pluginInfo.Instance.Config is ConfigFile config ? config.Count : -1)).Where(pluginInfo => pluginInfo.Metadata.GUID != Constants.GUID)];
        }

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            for (int i = 0; i < mods.Length; i++)
            {
                PluginInfo pluginInfo = mods[i];

                Widget_PushButton pushButton = new(OpenModInfo, pluginInfo)
                {
                    Colour = ColourPalette.Blue,
                    Symbol = InfoWatchSymbol.Info
                };

                if (stateSupportedMods.Contains(pluginInfo))
                {
                    bool isEnabled = pluginInfo.Instance.enabled;
                    lines.Append(pluginInfo.Metadata.Name).Append(": ").AppendColour(isEnabled ? "Enabled" : "Disabled", isEnabled ? ColourPalette.Green.Evaluate(0) : ColourPalette.Red.Evaluate(0)).Add(new Widget_Switch(isEnabled, ToggleMod, pluginInfo), pushButton);
                    continue;
                }

                lines.Add(pluginInfo.Metadata.Name, pushButton);
            }

            return lines;
        }

        private void ToggleMod(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is PluginInfo pluginInfo)
            {
                pluginInfo.Instance.enabled = value;
                SetText();
            }
        }

        private void OpenModInfo(object[] args)
        {
            if (args.ElementAtOrDefault(0) is PluginInfo info)
            {
                ModInfoPage.Mod = info;
                SetScreen<ModInfoPage>();
            }
        }
    }
}
