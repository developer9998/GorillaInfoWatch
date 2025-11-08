using GorillaInfoWatch.Models;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    internal class ShortcutHandler : MonoBehaviour
    {
        public static ShortcutHandler Instance { get; private set; }

        public Shortcut Shortcut => Watch.LocalWatch.shortcutButton.Shortcut;

        private const string _shortcutIdEntry = "ShortcutName";

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            Events.OnModInitialized += Initialize;
        }

        public void Initialize()
        {
            Shortcut lastShortcut = null;

            if (DataManager.Instance.HasEntry(_shortcutIdEntry))
            {
                string value = DataManager.Instance.GetEntry<string>(_shortcutIdEntry);

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
            if (shortcut != null) DataManager.Instance.SetEntry(_shortcutIdEntry, shortcut.GetShortcutId());
            else DataManager.Instance.RemoveEntry(_shortcutIdEntry);
        }

        public void ExcecuteShortcut(Shortcut shortcut)
        {
            shortcut.Method?.Invoke(!shortcut.HasState || shortcut.GetState());
        }
    }
}
