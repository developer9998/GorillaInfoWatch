using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Tools
{
    /// <summary>
    /// A class used to manage views (activation, displaying, events, etc)
    /// </summary>
    public class TabManager
    {
        public event Action<string> OnTextChanged;
        public event Action<Type, object[]> OnNewTabRequested;

        public ITab Tab;

        /// <summary>
        /// Sets the current tab to a new tab with a set of parameters
        /// </summary>
        public void SetTab(ITab tab, object[] Parameters)
        {
            Logging.Info(string.Concat("Setting tab ", nameof(tab), " of type ", tab.ExecutionType));

            if (tab.ExecutionType == ExecutionType.Viewable)
            {
                if (Tab != null)
                {
                    Tab.OnTextChanged -= OnTabTextChanged;
                    Tab.OnTabSwitchRequest -= OnTabSwitchRequest;
                }

                tab.OnTextChanged += OnTabTextChanged;
                tab.OnTabSwitchRequest += OnTabSwitchRequest;

                tab.OnTabDisplayed(Parameters);
                tab.OnScreenRefresh();

                Tab = tab;
            }
            else
            {
                tab.OnTabDisplayed(Parameters);
            }
        }

        private void OnTabTextChanged(string text) => OnTextChanged?.Invoke(text);
        private void OnTabSwitchRequest(Type type, object[] parameters) => OnNewTabRequested?.Invoke(type, parameters);
    }
}