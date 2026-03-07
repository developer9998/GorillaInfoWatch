using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen, PreserveSection]
    internal class ModListScreen : InfoScreen
    {
        public override string Title => "Mods";
        public override string Description => "View and inspect the list of your installed plugins";

        private MelonMod[] mods;

        public void Awake()
        {
            /*
            Dictionary<string, MelonMod> _loadedPlugins = Chainloader.PluginInfos;
            MelonMod[] _pluginInfos = [.. _loadedPlugins.Values];

            stateSupportedMods = [.. _pluginInfos.Where(delegate(MelonMod pluginInfo)
            {
                List<string> _instanceMethods = AccessTools.GetMethodNames(pluginInfo.Instance);
                return _instanceMethods.Contains("OnEnable") || _instanceMethods.Contains("OnDisable");
            }).OrderBy(pluginInfo => pluginInfo.Metadata.Name)];

            mods = [.. stateSupportedMods.Concat(_pluginInfos.Except(stateSupportedMods).OrderBy(pluginInfo => pluginInfo.Metadata.Name).OrderByDescending(pluginInfo => pluginInfo.Instance.Config is ConfigFile config ? config.Count : -1)).Where(pluginInfo => pluginInfo.Metadata.GUID != Constants.GUID)];

            // Set the active state of the necessary mods
            mods.ToDictionary(mod => mod, WatchManager.Instance.GetPersistentPluginState)
                .Where(element => (element.Key.Instance?.enabled ?? true) != element.Value)
                .ForEach(element => element.Key.Instance?.enabled = element.Value);
            */
        }

        public override InfoContent GetContent()
        {
            LineBuilder lines = new();

            for (int i = 0; i < mods.Length; i++)
            {
                MelonMod pluginInfo = mods[i];

                Widget_PushButton pushButton = new(OpenModInfo, pluginInfo)
                {
                    Colour = ColourPalette.Blue,
                    Symbol = Content.Shared.Symbols["Info"]
                };

                lines.Add(pluginInfo.Info.Name, pushButton);
            }

            return lines;
        }

        private void OpenModInfo(object[] args)
        {
            if (args.ElementAtOrDefault(0) is MelonMod info)
            {
                ModInspectorScreen.Mod = info;
                LoadScreen<ModInspectorScreen>();
            }
        }
    }
}
