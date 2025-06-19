using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Utilities;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public sealed class Widget_SnapSlider(int value, int start, int end, Action<int, object[]> action, params object[] parameters) : Widget_Base
    {
        public int Value = value;

        public Action<int, object[]> Command = action;

        public readonly object[] Parameters = parameters ?? [];

        public int StartValue = start;

        public int EndValue = end;

        public Gradient Colour = GradientUtils.FromColour(Gradients.Red.Evaluate(0), Gradients.Green.Evaluate(0));

        public bool ReadOnly;

        public Widget_SnapSlider(int value, int start, int end, Action<int> action) : this(value, start, end, (value, parameters) => action(value))
        {
            // Must declare a body
        }

        public override void Object_Construct(InfoWatchLine menuLine)
        {
            if (gameObject == null || !gameObject)
            {
                gameObject = UnityEngine.Object.Instantiate(menuLine.SnapSlider.gameObject, menuLine.SnapSlider.transform.parent);
                gameObject.name = "SnapSlider";
                gameObject.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (gameObject && gameObject.TryGetComponent(out SnapSlider component))
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