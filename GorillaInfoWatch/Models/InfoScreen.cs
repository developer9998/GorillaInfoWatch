using System;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public abstract class InfoScreen : MonoBehaviour
    {
        public abstract string Title { get; }
        public virtual string Description { get; set; }
        public virtual Type ReturnType { get; set; } = null;

        public event Action<bool> UpdateScreenRequest;

        public event Action<Type> LoadScreenRequest;

        public Type CallerScreenType;

        internal int SectionNumber;

        internal InfoContent Content;

        public void LoadScreen<T>() where T : InfoScreen => LoadScreen(typeof(T));
        public void LoadScreen(Type type) => LoadScreenRequest?.Invoke(type);
        public void ReturnScreen() => LoadScreenRequest?.Invoke(null);

        public abstract InfoContent GetContent();

        public void SetText() => UpdateScreenRequest?.Invoke(false);
        public void SetContent() => UpdateScreenRequest?.Invoke(true);

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
