using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Interfaces;
using GorillaInfoWatch.Models.Shortcuts;
using GorillaInfoWatch.Shortcuts.Rooms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

[assembly: ShortcutCategory("Rooms", typeof(JoinRandomRoom), typeof(JoinSpecificRoom), typeof(Leave), typeof(Rejoin), typeof(CopyRoomName))]

namespace GorillaInfoWatch.Behaviours;

internal class ShortcutHandler : MonoBehaviour, IInitializeCallback
{
    public static ShortcutHandler Instance { get; private set; }
    public static Shortcut Shortcut => Watch.LocalWatch.shortcutButton.Shortcut;

    public ReadOnlyCollection<Shortcut> Shortcuts => _shortcuts.AsReadOnly();
    private readonly List<Shortcut> _shortcuts = [];

    public ReadOnlyCollection<ShortcutCategory> Categories => _categories.AsReadOnly();

    private readonly List<ShortcutCategory> _categories = [];

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Initialize()
    {
        foreach (PluginInfo pluginInfo in Chainloader.PluginInfos.Values)
        {
            Assembly assembly = pluginInfo?.Instance?.GetType().Assembly;
            var attributes = assembly.GetCustomAttributes<ShortcutCategory>();

            foreach (var attribute in attributes)
            {
                attribute.assembly = assembly;

                var list = new List<Shortcut>();

                foreach (var type in attribute.ShortcutTypes)
                {
                    if (type == null || !typeof(Shortcut).IsAssignableFrom(type)) continue;

                    var shortcut = (Shortcut)Activator.CreateInstance(type);
                    shortcut.Assembly = assembly;
                    _shortcuts.Add(shortcut);
                    list.Add(shortcut);
                }

                _categories.Add(attribute);
                attribute.shortcuts = list;
            }
        }

        Shortcut lastShortcut = null;

        if (DataManager.Instance.HasData(Constants.DataEntry_ShortcutName))
        {
            string value = DataManager.Instance.GetData<string>(Constants.DataEntry_ShortcutName);

            foreach (Shortcut shortcut in Shortcuts)
            {
                if (value.StartsWith(shortcut.GetShortcutId()))
                {
                    lastShortcut = shortcut;
                    break;
                }
            }
        }

        SetShortcut(lastShortcut, false);
    }

    public void SetOrRemoveShortcut(Shortcut shortcut)
    {
        if (Shortcut != shortcut) SetShortcut(shortcut);
        else RemoveShortcut();
    }

    public void RemoveShortcut() => SetShortcut(null);

    public void SetShortcut(Shortcut shortcut, bool saveShortcut = true)
    {
        Watch.LocalWatch.shortcutButton.SetShortcut(shortcut);
        if (saveShortcut) SaveShortcut(shortcut);
    }

    private void SaveShortcut(Shortcut shortcut)
    {
        if (shortcut != null) DataManager.Instance.SetData(Constants.DataEntry_ShortcutName, shortcut.GetShortcutId());
        else DataManager.Instance.RemoveData(Constants.DataEntry_ShortcutName);
    }

    public void ExcecuteShortcut(Shortcut shortcut)
    {
        shortcut.Invoke(!shortcut.HasState || shortcut.State);
    }
}
