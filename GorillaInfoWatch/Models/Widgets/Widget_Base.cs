using System;
using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Tools;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets;

public class Widget_Base
{
    public WidgetAlignment Alignment = WidgetAlignment.Right;

    internal WidgetController Controller = null;

    public object[] ControllerParameters = null;

    public Type ControllerType = null;

    public         GameObject Object;
    public virtual float      Width        { get; } = 20.5f;
    public virtual float      Depth        { get; } = 0f;
    public virtual bool       Modification { get; } = true;

    public virtual void Initialize(WatchLine menuLine) { }

    public virtual void Modify() { }

    public virtual bool Equals(Widget_Base widget) => false;

    ~Widget_Base() => Logging.Message($"Finalizing Widget: {GetType().Name}");
}