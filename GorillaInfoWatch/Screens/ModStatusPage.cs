using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    [DisplayAtHomeScreen]
    public class ModStatusPage : WatchScreen
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
                return _instanceMethods.Contains("OnEnable") && _instanceMethods.Contains("OnDisable");
            })];
            _modEntries = [.. _eligiblePlugins.Concat(_pluginInfos.Except(_eligiblePlugins))]; // sort
        }

        private bool IsEligible(PluginInfo info) => _eligiblePlugins.Contains(info);

        public override void OnScreenOpen()
        {
            Draw();
        }

        public void Draw()
        {
            LineBuilder = new();

            for (int i = 0; i < _modEntries.Length; i++)
            {
                PluginInfo info = _modEntries[i];
                if (IsEligible(info))
                {
                    bool isEnabled = info.Instance.enabled;
                    LineBuilder.AddLine(string.Format("{0} [<color=#{1}>{2}</color>]", info.Metadata.Name, isEnabled ? "00FF00" : "FF0000", isEnabled ? "E" : "D"), new WidgetButton(WidgetButton.EButtonType.Switch, !isEnabled, OnButtonSelect, i));
                    continue;
                }
                LineBuilder.AddLine(info.Metadata.Name);
            }
        }

        private void OnButtonSelect(bool value, object[] args)
        {
            if (args[0] is int mod_index)
            {
                _modEntries[mod_index].Instance.enabled = !value;
                Draw();
                SetLines();
            }
        }
    }
}
