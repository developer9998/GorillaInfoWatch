using BepInEx;
using BepInEx.Bootstrap;
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
        public override string Title => "Mod Status";

        private static PluginInfo[] _eligiblePlugins, _modEntries;

        public void Awake()
        {
            Dictionary<string, PluginInfo> _loadedPlugins = Chainloader.PluginInfos;
            PluginInfo[] _pluginInfos = [.. _loadedPlugins.Values];

            _eligiblePlugins = [.. _pluginInfos.Where(plugin =>
            {
                List<string> _instanceMethods = AccessTools.GetMethodNames(plugin.Instance);
                return _instanceMethods.Contains("OnEnable") || _instanceMethods.Contains("OnDisable");
            })];
            _modEntries = [.. _eligiblePlugins.Concat(_pluginInfos.Except(_eligiblePlugins))]; // sort
        }

        private bool IsEligible(PluginInfo info) => _eligiblePlugins.Contains(info);


        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            for (int i = 0; i < _modEntries.Length; i++)
            {
                PluginInfo info = _modEntries[i];

                Widget_PushButton pushButton = new(OpenModInfo, info);

                if (IsEligible(info))
                {
                    bool isEnabled = info.Instance.enabled;
                    lines.Add(string.Format("{0} [<color=#{1}>{2}</color>]", info.Metadata.Name, isEnabled ? "00FF00" : "FF0000", isEnabled ? "E" : "D"), new Widget_Switch(isEnabled, ToggleMod, i), pushButton);
                    continue;
                }

                lines.Add(info.Metadata.Name, pushButton);
            }

            return lines;
        }

        private void ToggleMod(bool value, object[] args)
        {
            if (args[0] is int mod_index)
            {
                _modEntries[mod_index].Instance.enabled = value;
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
