using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using System;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Tabs
{
    public class SettingsEntry : IEntry
    {
        public string Name => "Settings";
        public Type Window => typeof(SettingsWindow);
    }

    public class SettingsWindow : Window
    {
        private readonly ItemHandler ItemHandler;
        private readonly Configuration Config;

        public SettingsWindow(Configuration config)
        {
            ItemHandler = new ItemHandler(1);
            Config = config;
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();
            str.AppendLine("- Config -".AlignCenter(Constants.Width)).AppendLine();

            str.AppendItem(Config.RefreshRate.ToDetailedString(0, 10), 0, ItemHandler);

            SetText(str);
        }

        private void OnEntryAdjusted(int entry, bool increase)
        {
            switch (entry)
            {
                case 0:
                    int increment = increase ? 2 : -2;
                    Config.RefreshRate.Value = Mathf.Clamp(Config.RefreshRate.Value + increment, 0, 10);
                    break;
            }
        }

        public override void OnButtonPress(ButtonType type)
        {
            if (ItemHandler.HandleButton(type))
            {
                OnScreenRefresh();
                return;
            }

            if (type == ButtonType.Left || type == ButtonType.Right)
            {
                OnEntryAdjusted(ItemHandler.CurrentEntry, type == ButtonType.Right);
                OnScreenRefresh();
            }

            if (type == ButtonType.Back)
            {
                DisplayWindow<HomeWindow>();
            }
        }
    }
}
