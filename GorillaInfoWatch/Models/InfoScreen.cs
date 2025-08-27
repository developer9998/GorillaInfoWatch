using GorillaInfoWatch.Screens;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public abstract class InfoScreen : MonoBehaviour
    {
        public abstract string Title { get; }
        public virtual string Description { get; set; }
        public virtual Type ReturnType { get; set; } = null;

        public event Action<bool> UpdateScreenEvent;

        public event Action<Type> LoadScreenRequest;

        internal int sectionNumber;

        internal InfoContent contents;

        public void ReturnToHomePage() => LoadScreen<HomeScreen>();
        public void LoadScreen<T>() where T : InfoScreen => LoadScreen(typeof(T));
        public void LoadScreen(Type type) => LoadScreenRequest?.Invoke(type);

        public abstract InfoContent GetContent();

        public void SetText() => UpdateScreenEvent?.Invoke(false);
        public void SetContent() => UpdateScreenEvent?.Invoke(true);

        public virtual void OnScreenLoad()
        {

        }

        public virtual void OnScreenUnload()
        {

        }

        public virtual void OnScreenReload()
        {

        }
    }
}
