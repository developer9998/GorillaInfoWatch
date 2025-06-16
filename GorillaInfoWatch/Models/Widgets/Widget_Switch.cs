using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Utilities;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_Switch(Action<bool, object[]> action, params object[] parameters) : Widget_Base
    {
        public bool Value;

        public readonly Action<bool, object[]> Command = action;

        public readonly object[] Parameters = parameters ?? [];

        public Gradient Colour = GradientUtils.FromColour(Gradients.Red.Evaluate(0), Gradients.Green.Evaluate(0));

        public Widget_Switch(Action<bool> action) : this((value, args) => action(value), [])
        {
            // Must declare a body
        }

        public Widget_Switch(Action<object[]> action, params object[] parameters) : this((value, args) => action(args), parameters)
        {
            // Must declare a body
        }

        public override void Object_Construct(InfoWatchLine menuLine)
        {
            if (Object is null)
            {
                Object = UnityEngine.Object.Instantiate(menuLine.Switch.gameObject, menuLine.Switch.transform.parent);
                Object.name = "Switch";
                Object.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (Object is not null && Object.TryGetComponent(out SwitchComponent component))
                component.SetWidget(this);
        }

        public override bool Equals(Widget_Base widget)
        {
            if (widget is null)
                return false;

            if (widget is not Widget_Switch widgetButton)
                return false;

            return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
        }
    }
}
