using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Utilities;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_Switch(bool value, Action<bool, object[]> action, params object[] parameters) : Widget_Base
    {
        public bool Value = value;

        public readonly Action<bool, object[]> Command = action;

        public readonly object[] Parameters = parameters ?? [];

        public Gradient Colour = GradientUtils.FromColour(Gradients.Red.Evaluate(0), Gradients.Green.Evaluate(0));

        public Widget_Switch(bool value, Action<bool> action) : this(value, (value, args) => action(value), [])
        {
            // Must declare a body
        }

        public Widget_Switch(bool value, Action<object[]> action, params object[] parameters) : this(value, (value, args) => action(args), parameters)
        {
            // Must declare a body
        }

        public override void Object_Construct(InfoWatchLine menuLine)
        {
            if (gameObject is null && !gameObject)
            {
                gameObject = UnityEngine.Object.Instantiate(menuLine.Switch.gameObject, menuLine.Switch.transform.parent);
                gameObject.name = "Switch";
                gameObject.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (gameObject && gameObject.TryGetComponent(out Switch component))
                component.AssignWidget(this);
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
