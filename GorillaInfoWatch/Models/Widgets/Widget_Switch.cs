using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public sealed class Widget_Switch(bool value, Action<bool, object[]> action, params object[] parameters) : Widget_Base
    {
        public override float Width => 25.5f;
        public bool IsReadOnly => Command == null || Command.Target == null;

        public bool Value = value;
        public readonly Action<bool, object[]> Command = action;
        public readonly object[] Parameters = parameters ?? [];

        public Gradient Colour = ColourPalette.CreatePalette(ColourPalette.Red.GetInitialColour(), ColourPalette.Green.GetInitialColour());

        public Widget_Switch(bool value) : this(value, (Action<bool, object[]>)null, null)
        {
            // Must declare a body
        }

        public Widget_Switch(bool value, Action<bool> action) : this(value, (value, args) => action(value), [])
        {
            // Must declare a body
        }

        public Widget_Switch(bool value, Action<object[]> action, params object[] parameters) : this(value, (value, args) => action(args), parameters)
        {
            // Must declare a body
        }

        public override void Initialize(PanelLine menuLine)
        {
            if (Object == null || !Object)
            {
                Object = UnityEngine.Object.Instantiate(menuLine.Switch.gameObject, menuLine.Switch.transform.parent);
                Object.name = "Switch";
                Object.SetActive(true);
            }
        }

        public override void Modify()
        {
            if (Object && Object.TryGetComponent(out Switch component))
                component.AssignWidget(this);
        }

        public override bool Equals(Widget_Base widget)
        {
            return true;

            /*
            if (widget is null)
                return false;

            if (widget is not Widget_Switch widgetButton)
                return false;

            return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
            */
        }
    }
}
