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
    public class ConfigEntry : IEntry
    {
        public string Name => "Configuration";
        public Type EntryType => typeof(ConfigTab);
    }

    public class ConfigTab : Tab
    {
        private readonly ItemHandler ItemHandler;
        private readonly Configuration Config;

        public ConfigTab(Configuration config)
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

        private void OnModfied(int index, bool increase)
        {
            switch (index)
            {
                case 0:
                    int increment = increase ? 2 : -2;
                    Config.RefreshRate.Value = Mathf.Clamp(Config.RefreshRate.Value + increment, 0, 10);
                    break;
            }
        }

        public override void OnButtonPress(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Up:
                    ItemHandler.Change(-1);
                    break;
                case ButtonType.Down:
                    ItemHandler.Change(1);
                    break;
                case ButtonType.Left:
                case ButtonType.Right:
                    OnModfied(ItemHandler.CurrentEntry, type == ButtonType.Right);
                    break;
                case ButtonType.Back:
                    DisplayTab<MainTab>();
                    return;
                default:
                    return;
            }

            OnScreenRefresh();
        }
    }
}
