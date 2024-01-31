using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorillaInfoWatch.Tabs
{
    public class ModStatusEntry : IEntry
    {
        public string Name => "Mod Status";
        public Type Window => typeof(ModStatusWindow);
    }

    public class ModStatusWindow : Window
    {
        private readonly List<PluginInfo> PluginList;
        private readonly PageHandler<PluginInfo> PageHandler = new();

        public ModStatusWindow()
        {
            PluginList = new();

            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                var pluginMethodList = AccessTools.GetMethodNames(pluginInfo.Instance);

                bool isEligible = pluginMethodList.Contains("OnEnable") && pluginMethodList.Contains("OnDisable");
                if (isEligible) PluginList.Add(pluginInfo);
            }

            PageHandler = new PageHandler<PluginInfo>()
            {
                Items = PluginList,
                EntriesPerPage = 8
            };
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();
            str.AppendLine($"- Mod Status -".AlignCenter(Constants.Width)).AppendLine();

            if (PageHandler.Items.Count == 0)
            {
                str.AppendLine("<color=red>You don't have any elgible mods installed.</color>");
            }
            else
            {
                List<PluginInfo> PluginCollection = PageHandler.GetItemsAtEntry();
                for (int i = 0; i < PluginCollection.Count; i++)
                {
                    int index = i + (PageHandler.PageNumber() * PageHandler.EntriesPerPage);
                    str.AppendItem(string.Concat(PluginCollection[i].Metadata.Name, " [", PluginCollection[i].Instance.enabled ? "<color=lime>E</color>" : "<color=red>D</color>", "]"), index, PageHandler);
                }

                str.Append(string.Concat(Enumerable.Repeat("\n", PageHandler.EntriesPerPage - PluginCollection.Count))).AppendLine();
                str.Append(string.Concat(" Page ", PageHandler.PageNumber() + 1, "/", PageHandler.PageCount()));
            }

            SetText(str);
        }

        public override void OnButtonPress(ButtonType type)
        {
            if (PageHandler.HandleButton(type))
            {
                OnScreenRefresh();
                return;
            }

            switch (type)
            {
                case ButtonType.Enter:
                    PluginInfo plugin = PageHandler.Items[PageHandler.CurrentEntry];
                    plugin.Instance.enabled ^= true;
                    OnScreenRefresh();
                    break;
                case ButtonType.Back:
                    DisplayWindow<HomeWindow>();
                    break;
            }
        }
    }
}
