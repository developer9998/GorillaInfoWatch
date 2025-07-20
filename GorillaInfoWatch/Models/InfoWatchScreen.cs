using GorillaInfoWatch.Screens;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public abstract class InfoWatchScreen : MonoBehaviour
    {
        public abstract string Title { get; }
        public virtual string Description { get; set; }

        public int Section;
        public ScreenContent Content;

        public event Action<bool> RequestSetLines;
        public event Action<Type> RequestScreenSwitch;

        public void ReturnToHomePage() => SetScreen<HomeScreen>();
        public void SetScreen<T>() where T : InfoWatchScreen => SetScreen(typeof(T));
        public void SetScreen(Type type) => RequestScreenSwitch?.Invoke(type);

        public void SetText() => SetContent(false);
        public void SetContent(bool setWidgets = true) => RequestSetLines?.Invoke(setWidgets);
        public abstract ScreenContent GetContent();

        public virtual void OnShow()
        {

        }

        public virtual void OnClose()
        {

        }

        public virtual void OnRefresh()
        {

        }
    }
}
