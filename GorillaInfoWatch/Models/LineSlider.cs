using System;

namespace GorillaInfoWatch.Models
{
    public class LineSlider
    {
        public int ReturnIndex, Split, InitialValue;

        public delegate void EventHandler(object sender, SliderArgs args);
        public event EventHandler OnApplied;

        public LineSlider(EventHandler onApplied, int returnIndex, int split, int initialValue = 0)
        {
            OnApplied = onApplied;
            ReturnIndex = returnIndex;
            Split = split;
            InitialValue = initialValue;
        }

        public void RaiseEvent(int currentValue)
        {
            SliderArgs args = new()
            {
                currentValue = currentValue,
                returnIndex = ReturnIndex,
                split = Split
            };
            OnApplied?.Invoke(this, args);
        }

        public bool Equals(LineSlider other) => other != null && other.ReturnIndex == this.ReturnIndex && other.Split == this.Split && InitialValue == other.InitialValue;
    }

    public class SliderArgs : EventArgs
    {
        public int returnIndex, split, currentValue;
    }
}
