using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_PushButton(Action<object[]> action, params object[] parameters) : Widget_Base
    {
        public Action<object[]> Command = action;

        public object[] Parameters = parameters;

        public Gradient Colour = Gradients.Button;

        public Widget_PushButton(Action action) : this(args => action(), [])
        {
            // Must declare a body
        }

        public override void Object_Construct(InfoWatchLine menuLine)
        {
            if (Object is null)
            {
                Object = UnityEngine.Object.Instantiate(menuLine.Button.gameObject, menuLine.Button.transform.parent);
                Object.name = "Button";
                Object.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (Object is not null && Object.TryGetComponent(out PushButtonComponent component))
                component.ApplyButton(this);
        }

        public override bool Equals(Widget_Base widget)
        {
            if (widget is null)
                return false;

            if (widget is not Widget_PushButton widgetButton)
                return false;

            return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
        }
    }
}