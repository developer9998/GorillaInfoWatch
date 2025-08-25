using GorillaInfoWatch.Screens;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public abstract class Screen : MonoBehaviour
    {
        public abstract string Title { get; }
        public virtual string Description { get; set; }
        public virtual Type ReturnType { get; set; } = null;

        internal int Section;
        internal ScreenLines Lines;

        public event Action<bool> UpdateScreenEvent;
        public event Action<Type> ScreenSwitchEvent;

        public void ReturnToHomePage() => SetScreen<HomeScreen>();
        public void SetScreen<T>() where T : Screen => SetScreen(typeof(T));
        public void SetScreen(Type type) => ScreenSwitchEvent?.Invoke(type);

        public void SetText() => SetContent(false);
        public void SetContent(bool setWidgets = true) => UpdateScreenEvent?.Invoke(setWidgets);
        public abstract ScreenLines GetContent();

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
