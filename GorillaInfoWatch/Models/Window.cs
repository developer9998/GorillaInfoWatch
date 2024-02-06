using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Utilities;
using GorillaInfoWatch.Windows;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Window : IWindow
    {
        public string Text { get; set; }
        public Type CallerWindow { get; set; }
        public virtual WindowExecutionType ExecutionType { get; }

        public event Action<string> OnTextChanged;

        public event Action<Color> OnMenuColourRequest;

        public event Action<Type, Type, object[]> OnWindowSwitchRequest;

        public void SetText(object text)
        {
            Text = text.ToString();
            OnTextChanged?.Invoke(Text);
        }

        public void SetMenuColour(PresetColours type) => SetMenuColour(PresetUtils.Parse(type));
        public void SetMenuColour(Color colour) => OnMenuColourRequest?.Invoke(colour);

        public void ReturnHome() => DisplayWindow(typeof(HomeWindow));
        public void ReturnWindow() => DisplayWindow(CallerWindow);

        public void DisplayWindow<T>() => DisplayWindow(typeof(T));
        public void DisplayWindow(Type type) => OnWindowSwitchRequest?.Invoke(GetType(), type, null);
        public void DisplayWindow(Type type, object[] parameters) => OnWindowSwitchRequest?.Invoke(GetType(), type, parameters);

        public virtual void OnWindowDisplayed(object[] Parameters) => OnTextChanged?.Invoke(Text);

        public virtual void OnScreenRefresh()
        {

        }

        public virtual void OnButtonPress(InputType type)
        {

        }
    }
}
