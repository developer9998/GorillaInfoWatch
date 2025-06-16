using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Utilities;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Switch : Widget
    {
        public bool Value;

        public readonly Action<bool, object[]> Command;

        public readonly object[] Parameters;

        public Gradient Colour;

        public Switch(Action<bool, object[]> action, params object[] parameters)
        {
            Command = action;
            Parameters = parameters ?? [];

            Colour = GradientUtils.FromColour(Gradients.Red.colorKeys[0].color, Gradients.Green.colorKeys[0].color);
        }

        public Switch(Action<bool> action) : this((value, args) => action(value), [])
        {
            // Must declare a body
        }

        public Switch(Action<object[]> action, params object[] parameters) : this((value, args) => action(args), parameters)
        {
            // Must declare a body
        }

        public override void CreateObject(InfoWatchLine menuLine)
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
