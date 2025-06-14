using GorillaInfoWatch.Behaviours.Widgets;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Switch : Widget
    {
        public override EWidgetType WidgetType => EWidgetType.Interaction;

        public bool Value;

        public readonly Action<bool, object[]> Command;

        public readonly object[] Parameters;

        public Gradient Colour;

        public Switch(Action<bool, object[]> action, params object[] parameters)
        {
            Command = action;
            Parameters = parameters ?? [];

            Colour = new Gradient();
            Colour.SetKeys
            (
                [new GradientColorKey(new Color32(188, 67, 67, 255), 0), new GradientColorKey(new Color32(67, 188, 84, 255), 1)],
                [new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)]
            );
        }

        public Switch(Action<bool> action) : this((value, args) => action(value), [])
        {
            // Must declare a body
        }

        public Switch(Action<object[]> action, params object[] parameters) : this((value, args) => action(args), parameters)
        {
            // Must declare a body
        }

        public override void CreateObject(Behaviours.Line menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.Switch.gameObject, menuLine.Switch.transform.parent);
            gameObject.name = "Switch";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out SwitchComponent component))
                component.SetWidget(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not Switch widgetButton)
                return false;

            return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
        }
    }
}
