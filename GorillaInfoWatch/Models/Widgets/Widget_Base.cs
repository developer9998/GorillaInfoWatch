using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets;

public class Widget_Base
{
    public virtual float Width { get; } = 20.5f;
    public virtual float Depth { get; } = 0f;
    public virtual bool Modification { get; } = true;

    public GameObject Object;

    public WidgetAlignment Alignment = WidgetAlignment.Right;

    public Type ControllerType = null;

    public object[] ControllerParameters = null;

    internal WidgetController Controller = null;

    public virtual void Initialize(PanelLine menuLine)
    {

    }

    public virtual void Modify()
    {

    }

    public virtual bool Equals(Widget_Base widget) => false;

    // ~Widget_Base() => Logging.Message($"Finalizing Widget: {GetType().Name}");
}
