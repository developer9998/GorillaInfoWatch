using GorillaInfoWatch.Models;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Interfaces
{
    public interface IWindow
    {
        /// <summary>
        /// The text which is displayed on the window
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The type of the window which displayed this window
        /// </summary>
        Type CallerWindow { get; set; }

        /// <summary>
        /// The type of execution the window uses when displayed
        /// </summary>
        ExecutionType ExecutionType { get; }

        event Action<string> OnTextChanged;
        event Action<Color> OnMenuColourRequest;

        event Action<Type, Type, object[]> OnWindowSwitchRequest;

        void OnWindowDisplayed(object[] Parameters);
        void OnScreenRefresh();
        void OnButtonPress(ButtonType type);
    }
}
