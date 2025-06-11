using System;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Widgets;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class PushButton : Widget
    {
        public override EWidgetType WidgetType => EWidgetType.Interaction;

        public Action<object[]> Command;

        public object[] Parameters;

        public Gradient Colour;

        public PushButton(Action<object[]> action, params object[] parameters)
        {
            Command = action;
            Parameters = parameters;

            Colour = new Gradient();
            Colour.SetKeys
            (
                [new GradientColorKey(new Color32(191, 188, 170, 255), 0), new GradientColorKey(new Color32(132, 131, 119, 255), 1)],
                [new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)]
            );
        }

        public PushButton(Action action) : this(args => action(), [])
        {
            // Must declare a body
        }

        public override void CreateObject(MenuLine menuLine)
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