using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Mods")]
    public class ModStatusPage : Page
    {
        private static PluginInfo[] _eligiblePlugins, _modEntries;

        public ModStatusPage()
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

        public override void OnDisplay()
        {
            base.OnDisplay();

            SetHeader("Mods", string.Format("Loaded {0}/{1} Togglable Mods", _eligiblePlugins.Length, _modEntries.Length));

            DrawPage();
            SetLines();
        }

        private void DrawPage()
        {
            ClearLines();

            for (int i = 0; i < _modEntries.Length; i++)
            {
                PluginInfo info = _modEntries[i];
                if (IsEligible(info))
                {
                    bool isEnabled = info.Instance.enabled;
                    AddLine(string.Format("{0} [<color=#{1}>{2}</color>]", info.Metadata.Name, isEnabled ? "00FF00" : "FF0000", isEnabled ? "E" : "D"), new LineButton(OnButtonSelect, i));
                    continue;
                }
                AddLine(info.Metadata.Name);
            }
        }

        private void OnButtonSelect(object sender, ButtonArgs args)
        {
            _eligiblePlugins[args.returnIndex].Instance.enabled ^= true;

            DrawPage();
            UpdateLines();
        }
    }
}
