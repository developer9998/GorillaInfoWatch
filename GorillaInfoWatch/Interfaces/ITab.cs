using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface ITab
    {
        string Text { get; set; }
        ExecutionType ExecutionType { get; }

        event Action<string> OnTextChanged;

        event Action<Type, object[]> OnTabSwitchRequest;

        void OnTabDisplayed(object[] Parameters);
        void OnScreenRefresh();
        void OnButtonPress(ButtonType type);
    }
}
