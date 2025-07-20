using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public sealed class Widget_PushButton(Action<object[]> action, params object[] parameters) : Widget_Base
    {
        public bool IsReadOnly => Command == null || Command.Target == null;

        public Action<object[]> Command = action;
        public object[] Parameters = parameters;

        public Gradient Colour = ColourPalette.Button;
        public Symbol Symbol;

        public Widget_PushButton() : this(null, null)
        {

        }

        public Widget_PushButton(Action action) : this(args => action(), [])
        {
            // Must declare a body
        }

        public override void Object_Construct(WatchLine menuLine)
        {
            if (gameObject == null || !gameObject)
            {
                gameObject = UnityEngine.Object.Instantiate(menuLine.Button.gameObject, menuLine.Button.transform.parent);
                gameObject.name = "Button";
                gameObject.SetActive(true);
            }
        }

        public override void Object_Modify()
        {
            if (gameObject && gameObject.TryGetComponent(out PushButton component))
                component.AssignWidget(this);
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