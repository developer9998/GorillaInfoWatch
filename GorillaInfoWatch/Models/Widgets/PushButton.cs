using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class PushButton(Action<object[]> action, params object[] parameters) : Widget
    {
        public Action<object[]> Command = action;

        public object[] Parameters = parameters;

        public Gradient Colour = Gradients.Button;

        public PushButton(Action action) : this(args => action(), [])
        {
            // Must declare a body
        }

        public override void CreateObject(InfoWatchLine menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.Button.gameObject, menuLine.Button.transform.parent);

            gameObject.name = "Button";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out PushButtonComponent component))
                component.ApplyButton(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not PushButton widgetButton)
                return false;

            return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
        }
    }
}