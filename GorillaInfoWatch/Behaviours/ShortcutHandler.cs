using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Interfaces;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours;

internal class ShortcutHandler : MonoBehaviour, IInitialize
{
    public static ShortcutHandler Instance { get; private set; }

    public Shortcut Shortcut => Watch.LocalWatch.shortcutButton.Shortcut;

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
        Shortcut lastShortcut = null;

        if (DataManager.Instance.HasData(Constants.DataEntry_ShortcutName))
        {
            string value = DataManager.Instance.GetData<string>(Constants.DataEntry_ShortcutName);

            foreach (Shortcut shortcut in ShortcutRegistrar.AllShortcuts)
            {
                if (shortcut.GetShortcutId() == value)
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
        shortcut.Method?.Invoke(!shortcut.HasState || shortcut.GetState());
    }
}
