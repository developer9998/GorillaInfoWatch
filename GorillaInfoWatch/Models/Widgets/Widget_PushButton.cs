using System;
using GorillaInfoWatch.Behaviours.UI;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets;

public sealed class Widget_PushButton(Action<object[]> action, params object[] parameters) : Widget_Base
{
    public Gradient Colour = ColourPalette.Button;

    public Action<object[]> Command    = action;
    public object[]         Parameters = parameters;
    public Symbol           Symbol;

    public Widget_PushButton() : this(null, null) { }

    public Widget_PushButton(Action action) : this(args => action())
    {
        // Must declare a body
    }

    public bool IsReadOnly => Command == null || Command.Target == null;

    public override void Initialize(WatchLine menuLine)
    {
        if (Object == null || !Object)
        {
            Object      = UnityEngine.Object.Instantiate(menuLine.Button.gameObject, menuLine.Button.transform.parent);
            Object.name = "Button";
            Object.SetActive(true);
        }
    }

    public override void Modify()
    {
        if (Object && Object.TryGetComponent(out PushButton component))
            component.AssignWidget(this);
    }

    public override bool Equals(Widget_Base widget) => true;
    /*
        if (widget is null)
            return false;

        if (widget is not Widget_PushButton widgetButton)
            return false;

        return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
        */
}