using GorillaInfoWatch.Screens;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        public void ReturnToHomePage() => ShowScreen(typeof(HomeScreen));

        public void ShowScreen(Type type) => RequestScreenSwitch?.Invoke(type);

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
