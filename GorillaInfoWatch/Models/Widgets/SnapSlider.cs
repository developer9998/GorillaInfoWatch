using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class SnapSlider : Widget
    {
        public int Value;

        public Action<int> Command;

        public int StartValue;

        public int EndValue;

        public Gradient Colour;

        public bool ReadOnly;

        public SnapSlider(Action<int> action, int start = 0, int end = 100)
        {
            Command = action;
            StartValue = start;
            EndValue = end;
            Colour = GradientUtils.FromColour(Gradients.Red.Evaluate(0), Gradients.Green.Evaluate(0));
        }

        public override void Object_Construct(InfoWatchLine menuLine)
        {
            if (gameObject is null)
            {
                gameObject = UnityEngine.Object.Instantiate(menuLine.SnapSlider.gameObject, menuLine.SnapSlider.transform.parent);
                gameObject.name = "SnapSlider";
                gameObject.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (gameObject is not null && gameObject.TryGetComponent(out SnapSliderComponent component))
                component.ApplySlider(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not SnapSlider widgetSnapSlider)
                return false;

            return Command.Target == widgetSnapSlider.Command.Target && Command.Method.Equals(widgetSnapSlider.Command.Method) && StartValue == widgetSnapSlider.StartValue && EndValue == widgetSnapSlider.EndValue;
        }
    }
}