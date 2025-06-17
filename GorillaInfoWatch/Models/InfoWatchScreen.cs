using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public abstract class InfoWatchScreen : MonoBehaviour
    {
        public abstract string Title { get; }

        public virtual string Description { get; set; }

        public int Section = 0;

        public ScreenContent Content;

        public event Action<bool> RequestSetLines;

        public event Action<Type> RequestScreenSwitch;

        public Type CallerType;

        public void ReturnToHomePage() => SetScreen<HomeScreen>();

        public void SetScreen<T>() where T : InfoWatchScreen => SetScreen(typeof(T));

        public void SetScreen(Type type) => RequestScreenSwitch?.Invoke(type);

        public void SetText() => SetContent(false);

        public void SetContent(bool setWidgets = true) => RequestSetLines?.Invoke(setWidgets);

        public virtual void OnShow()
        {
            Logging.Info($"Show: {Title} / {GetType().Name})");
        }

        public virtual void OnClose()
        {
            Logging.Info($"Close: {Title} / {GetType().Name})");
        }

        public virtual void OnRefresh()
        {
            Logging.Info($"Refresh: {Title} / {GetType().Name})");
        }

        public abstract ScreenContent GetContent();
    }
}
