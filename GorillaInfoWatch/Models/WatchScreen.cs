using GorillaInfoWatch.Pages;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public abstract class WatchScreen : MonoBehaviour
    {
        public abstract string Title { get; }

        public virtual string Description { get; set; }

        public LineBuilder LineBuilder
        {
            get => (Content is LineBuilder line_builder) ? line_builder : null;
            set => Content = value;
        }

        public PageBuilder PageBuilder
        {
            get => (Content is PageBuilder page_builder) ? page_builder : null;
            set => Content = value;
        }

        public int PageNumber = 0;

        public ScreenContent Content;

        public event Action<bool> RequestSetLines;

        public event Action<Type> RequestScreenSwitch;

        public event Action RequestDisplayScreen;

        public void ReturnToHomePage() => ShowScreen(typeof(HomePage));

        public void ShowScreen(Type type) => RequestScreenSwitch?.Invoke(type);

        public void Display() => RequestDisplayScreen?.Invoke();

        public void SetLines(bool text_exclusive = false) => RequestSetLines?.Invoke(text_exclusive);

        public void UpdateLines() => SetLines(true);

        public virtual void OnScreenOpen()
        {

        }

        public virtual void OnScreenClose()
        {
            
        }
    }
}
