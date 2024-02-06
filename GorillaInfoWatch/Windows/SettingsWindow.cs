using BepInEx.Configuration;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Windows
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

        private readonly Dictionary<ConfigEntryBase, Action> Entries;

        private bool IsEditing;

        public SettingsWindow(Configuration config, Main main)
        {
            Config = config;

            Entries = new Dictionary<ConfigEntryBase, Action>()
            {
                {
                    Config.MenuColour,
                    () =>
                    {
                         main.SetBackground(PresetUtils.Parse(Config.MenuColour.Value));
                    }
                },
                {
                    Config.FavouriteColour,
                    () =>
                    {
                        ScoreboardUtils.GetActiveLines().DoIf(sL => sL.playerVRRig && sL.playerVRRig.playerText && sL.linePlayer != null && sL.linePlayer.InRoom(), sL => sL.playerVRRig.playerText.color = DataManager.GetItem(string.Concat(sL.linePlayer.UserId, "fav"), false, DataType.Stored) ? PresetUtils.Parse(Config.FavouriteColour.Value) : Color.white);
                        ScoreboardUtils.RedrawLines();
                    }
                },
                {
                    Config.TwFourHour,
                    null
                },
                {
                    Config.RefreshRate,
                    null
                },
                {
                    Config.ActivationVolume,
                    null
                },
                {
                    Config.ButtonVolume,
                    null
                }
            };

            ItemHandler = new ItemHandler(Entries.Count);
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();
            str.AppendLine("- Config -".AlignCenter(Constants.Width)).AppendLine();

            ConfigEntryBase currentEntry = Entries.Keys.ToArray()[ItemHandler.CurrentEntry];

            if (!IsEditing)
            {
                str.AppendSize("- Use the arrow keys to navigate through entries", 10).AppendLine();
                str.AppendSize("- Press the enter key to modify the current entry", 10).AppendLines(2);

                str.Append(currentEntry.Definition.Key)
                   .Append(": ").Append(currentEntry.BoxedValue.ToString())
                   .Append(currentEntry.SettingType == typeof(int) ? string.Concat(" ", AsciiUtils.Bar(10, (int)currentEntry.BoxedValue)) : "")
                   .Append(currentEntry.SettingType == typeof(float) ? string.Concat(" ", AsciiUtils.Bar(10, Mathf.RoundToInt((float)currentEntry.BoxedValue * 10))) : "")
                   .AppendLines(2);

                str.Append("Section: ").Append(currentEntry.Definition.Section).AppendLine();
                str.Append("Default Value: ").Append(currentEntry.DefaultValue.ToString()).AppendLines(2);

                str.AppendSize(string.Concat("<i>\"", currentEntry.Description.Description, "\"</i>"), 10).AppendLine();
            }
            else
            {
                bool isAdjustable = currentEntry.SettingType == typeof(int) || currentEntry.SettingType.IsEnum;
                str.AppendSize(isAdjustable ? "- Use the arrow keys to adjust the entry" : "- Use the enter key to set the entry", 10).AppendLine();
                str.AppendSize("- Press the back key to finalize the entry", 10).AppendLines(2);

                str.Append(currentEntry.Definition.Key)
                    .Append(": ").Append(currentEntry.BoxedValue.ToString())
                    .Append(currentEntry.SettingType == typeof(int) ? string.Concat(" ", AsciiUtils.Bar(10, (int)currentEntry.BoxedValue)) : "")
                    .Append(currentEntry.SettingType == typeof(float) ? string.Concat(" ", AsciiUtils.Bar(10, Mathf.RoundToInt((float)currentEntry.BoxedValue * 10))) : "")
                    .AppendLines(2);

                str.AppendSize(string.Concat("<i>\"", currentEntry.Description.Description, "\"</i>"), 10).AppendLine();
            }

            SetText(str);
        }

        private void OnEntryAdjusted(bool increase)
        {
            ConfigEntryBase currentEntry = Entries.Keys.ToArray()[ItemHandler.CurrentEntry];
            if (currentEntry.SettingType == typeof(int))
            {
                int increment = increase ? 1 : -1;
                currentEntry.BoxedValue = Mathf.Clamp((int)currentEntry.BoxedValue + increment, 0, 10);

                Config.Sync(currentEntry);
                Entries.Values.ToArray()[ItemHandler.CurrentEntry]?.Invoke();

                OnScreenRefresh();
            }
            else if (currentEntry.SettingType == typeof(float))
            {
                float increment = increase ? 0.1f : -0.1f;
                currentEntry.BoxedValue = Mathf.Clamp01((float)currentEntry.BoxedValue + increment);
                currentEntry.BoxedValue = Mathf.Round((float)currentEntry.BoxedValue * 10) / 10;

                Config.Sync(currentEntry);
                Entries.Values.ToArray()[ItemHandler.CurrentEntry]?.Invoke();

                OnScreenRefresh();
            }
            else if (currentEntry.SettingType.IsEnum)
            {
                int increment = increase ? 1 : -1;
                int enumLength = Enum.GetNames(currentEntry.SettingType).Length;

                currentEntry.BoxedValue = Mathf.Clamp((int)currentEntry.BoxedValue + increment, 0, enumLength - 1);

                Config.Sync(currentEntry);
                Entries.Values.ToArray()[ItemHandler.CurrentEntry]?.Invoke();

                OnScreenRefresh();
            }
        }

        public override void OnButtonPress(ButtonType type)
        {
            if (!IsEditing && ItemHandler.HandleButton(type))
            {
                OnScreenRefresh();
                return;
            }

            if (IsEditing && (type == ButtonType.Left || type == ButtonType.Right))
            {
                OnEntryAdjusted(type == ButtonType.Right);
                OnScreenRefresh();
            }

            switch (type)
            {
                case ButtonType.Enter:
                    if (!IsEditing)
                    {
                        IsEditing = true;
                        OnScreenRefresh();
                    }
                    else
                    {
                        ConfigEntryBase currentEntry = Entries.Keys.ToArray()[ItemHandler.CurrentEntry];
                        if (currentEntry.SettingType == typeof(bool))
                        {
                            currentEntry.BoxedValue = !(bool)currentEntry.BoxedValue;

                            Config.Sync(currentEntry);
                            Entries.Values.ToArray()[ItemHandler.CurrentEntry]?.Invoke();
                        }

                        OnScreenRefresh();
                    }
                    break;
                case ButtonType.Back:
                    if (!IsEditing)
                    {
                        DisplayWindow<HomeWindow>();
                    }
                    else
                    {
                        IsEditing = false;
                        OnScreenRefresh();
                    }
                    break;
            }
        }
    }
}
