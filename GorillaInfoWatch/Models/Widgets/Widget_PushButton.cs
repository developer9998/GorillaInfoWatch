using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Behaviours.UI.Widgets;
using GorillaInfoWatch.Extensions;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets;

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

    public override void Initialize(PanelLine menuLine)
    {
        if (Object.Null())
        {
            Object = UnityEngine.Object.Instantiate(menuLine.Button.gameObject, menuLine.Button.transform.parent);
            Object.name = "Button";
            Object.SetActive(true);
        }
    }

    public override void Modify()
    {
        if (Object.Exists() && Object.TryGetComponent(out PushButton component))
            component.AssignWidget(this);
    }

    public override bool Equals(Widget_Base widget)
    {
        return true;

        /*
        if (widget is null)
            return false;

        if (widget is not Widget_PushButton widgetButton)
            return false;

        return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
        */
    }
}