using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IWindow
    {
        string Text { get; set; }
        Type CallerWindow { get; set; }
        ExecutionType ExecutionType { get; }

        event Action<string> OnTextChanged;

        event Action<Type, Type, object[]> OnWindowSwitchRequest;

        void OnWindowDisplayed(object[] Parameters);
        void OnScreenRefresh();
        void OnButtonPress(ButtonType type);
    }
}
