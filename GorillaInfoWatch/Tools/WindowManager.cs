using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Tools
{
    /// <summary>
    /// A class used to manage windows (activation, displaying, events, etc)
    /// </summary>
    public class WindowManager
    {
        public event Action<string> OnTextChanged;
        public event Action<Color> OnBackgroundChanged;
        public event Action<Type, Type, object[]> OnWindowChanged;

        public IWindow Tab;

        /// <summary>
        /// Sets the current window to a new window with a set of parameters
        /// </summary>
        public void SetWindow(IWindow tab, object[] Parameters)
        {
            Logging.Info(string.Concat("Setting tab ", nameof(tab), " of type ", tab.ExecutionType));

            if (tab.ExecutionType == WindowExecutionType.Viewable)
            {
                if (Tab != null)
                {
                    Tab.OnTextChanged -= OnWindowTextChanged;
                    Tab.OnMenuColourRequest -= OnMenuColourRequest;
                    Tab.OnWindowSwitchRequest -= OnWindowSwitchRequest;
                }

                tab.OnTextChanged += OnWindowTextChanged;
                tab.OnMenuColourRequest += OnMenuColourRequest;
                tab.OnWindowSwitchRequest += OnWindowSwitchRequest;

                tab.OnWindowDisplayed(Parameters);
                tab.OnScreenRefresh();

                Tab = tab;
            }
            else
            {
                tab.OnWindowDisplayed(Parameters);
            }
        }

        private void OnWindowTextChanged(string text) => OnTextChanged?.Invoke(text);
        private void OnMenuColourRequest(Color colour) => OnBackgroundChanged?.Invoke(colour);
        private void OnWindowSwitchRequest(Type origin, Type type, object[] parameters) => OnWindowChanged?.Invoke(origin, type, parameters);
    }
}