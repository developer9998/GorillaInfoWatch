using System;

namespace GorillaInfoWatch.Models
{
    public class LineButton
    {
        public string Text;
        public int ReturnIndex;
        public bool UseToggle, InitialValue;

        public delegate void EventHandler(object sender, ButtonArgs args);
        public event EventHandler OnPressed;

        public void RaiseEvent(bool isActive)
        {
            ButtonArgs args = new()
            {
                returnIndex = ReturnIndex,
                currentValue = isActive,
                useToggle = UseToggle
            };
            OnPressed?.Invoke(this, args);
        }

        public LineButton(EventHandler onPressed, int returnIndex, bool useToggle = false, bool initialValue = false)
        {
            OnPressed = onPressed;
            ReturnIndex = returnIndex;
            UseToggle = useToggle;
            InitialValue = initialValue;
        }
    }

    [Serializable]
    public class ButtonArgs : EventArgs
    {
        public int returnIndex;
        public bool currentValue;
        public bool useToggle;
    }
}
