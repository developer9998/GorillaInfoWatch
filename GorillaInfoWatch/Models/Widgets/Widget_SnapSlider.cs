using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Utilities;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_SnapSlider(Action<int> action, int start = 0, int end = 100) : Widget_Base
    {
        public int Value;

        public Action<int> Command = action;

        public int StartValue = start;

        public int EndValue = end;

        public Gradient Colour = GradientUtils.FromColour(Gradients.Red.Evaluate(0), Gradients.Green.Evaluate(0));

        public bool ReadOnly;

        public override void Object_Construct(InfoWatchLine menuLine)
        {
            if (Object is null)
            {
                Object = UnityEngine.Object.Instantiate(menuLine.SnapSlider.gameObject, menuLine.SnapSlider.transform.parent);
                Object.name = "SnapSlider";
                Object.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (Object is not null && Object.TryGetComponent(out SnapSliderComponent component))
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