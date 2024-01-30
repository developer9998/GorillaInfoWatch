using GorillaInfoWatch.Interfaces;
using System;

namespace GorillaInfoWatch.Models
{
    public class Tab : ITab
    {
        public event Action<Type, object[]> OnTabSwitchRequest;
        public event Action<string> OnTextChanged;

        public string Text { get; set; }
        public virtual ExecutionType ExecutionType { get; }

        public void SetText(object text)
        {
            Text = text.ToString();
            OnTextChanged?.Invoke(Text);
        }

        public void DisplayTab<T>() => DisplayTab(typeof(T));
        public void DisplayTab(Type type) => OnTabSwitchRequest?.Invoke(type, null);
        public void DisplayTab(Type type, object[] parameters) => OnTabSwitchRequest?.Invoke(type, parameters);

        public virtual void OnTabDisplayed(object[] Parameters) => OnTextChanged?.Invoke(Text);

        public virtual void OnScreenRefresh()
        {

        }

        public virtual void OnButtonPress(ButtonType type)
        {

        }
    }
}
