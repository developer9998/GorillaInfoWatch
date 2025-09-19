using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public sealed class Widget_SnapSlider(int value, int start, int end, Action<int, object[]> action, params object[] parameters) : Widget_Base
    {
        public override float Width => 60f;
        public int Value = value;
        public Action<int, object[]> Command = action;
        public readonly object[] Parameters = parameters ?? [];
        public int StartValue = start;
        public int EndValue = end;

        public Gradient Colour = ColourPalette.CreatePalette(ColourPalette.Red.Evaluate(0), ColourPalette.Green.Evaluate(0));
        public bool ReadOnly;

        public Widget_SnapSlider(int value, int start, int end, Action<int> action) : this(value, start, end, (value, parameters) => action(value))
        {
            // Must declare a body
        }

        public override void Object_Construct(WatchLine menuLine)
        {
            if (Object == null || !Object)
            {
                Object = UnityEngine.Object.Instantiate(menuLine.SnapSlider.gameObject, menuLine.SnapSlider.transform.parent);
                Object.name = "SnapSlider";
                Object.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (Object && Object.TryGetComponent(out SnapSlider component))
                component.ApplySlider(this);
        }

        public override bool Equals(Widget_Base widget)
        {
            if (widget is null)
                return false;

            if (widget is not Widget_SnapSlider widgetSnapSlider)
                return false;

            return Command.Target == widgetSnapSlider.Command.Target && Command.Method.Equals(widgetSnapSlider.Command.Method) && StartValue == widgetSnapSlider.StartValue && EndValue == widgetSnapSlider.EndValue;
        }
    }
}