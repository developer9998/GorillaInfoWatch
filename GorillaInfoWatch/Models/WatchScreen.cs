﻿using System;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public abstract class WatchScreen : MonoBehaviour
    {
        public abstract string Title { get; }

        public virtual string Description { get; set; }

        /*
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
        */

        public int PageNumber = 0;

        // public ScreenContent Content;

        public event Action<bool> RequestSetLines;

        public event Action<Type> RequestScreenSwitch;

        public void ReturnToHomePage() => SetScreen<HomeScreen>();

        public void SetScreen<T>() where T : WatchScreen => SetScreen(typeof(T));

        public void SetScreen(Type type) => RequestScreenSwitch?.Invoke(type);

        public void SetText() => SetContent(false);

        public void SetContent(bool setWidgets = true) => RequestSetLines?.Invoke(setWidgets);

        public virtual void OnScreenOpen()
        {
            Logging.Info($"OnScreenOpen: {Title} / {GetType().Name})");
        }

        public virtual void OnScreenClose()
        {
            Logging.Info($"OnScreenClose: {Title} / {GetType().Name})");
        }

        public abstract ScreenContent GetContent();
    }
}
