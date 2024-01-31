using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Tools
{
    /// <summary>
    /// A class used to manage windows (activation, displaying, events, etc)
    /// </summary>
    public class WindowManager
    {
        public event Action<string> OnTextChanged;
        public event Action<Type, Type, object[]> OnNewTabRequested;

        public IWindow Tab;

        /// <summary>
        /// Sets the current window to a new window with a set of parameters
        /// </summary>
        public void SetWindow(IWindow tab, object[] Parameters)
        {
            Logging.Info(string.Concat("Setting tab ", nameof(tab), " of type ", tab.ExecutionType));

            if (tab.ExecutionType == ExecutionType.Viewable)
            {
                if (Tab != null)
                {
                    Tab.OnTextChanged -= OnWindowTextChanged;
                    Tab.OnWindowSwitchRequest -= OnWindowSwitchRequest;
                }

                tab.OnTextChanged += OnWindowTextChanged;
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
        private void OnWindowSwitchRequest(Type origin, Type type, object[] parameters) => OnNewTabRequested?.Invoke(origin, type, parameters);
    }
}