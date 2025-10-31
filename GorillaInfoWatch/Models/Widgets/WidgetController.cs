using System;
using GorillaInfoWatch.Tools;

namespace GorillaInfoWatch.Models.Widgets;

public abstract class WidgetController
{
    public          bool   Enabled = true;
    public abstract Type[] AllowedTypes { get; }

    public         Widget_Base Widget       { get; set; }
    public virtual float?      Depth        { get; }
    public virtual bool?       Modification { get; }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

    public virtual void Update() { }

    ~WidgetController() => Logging.Message($"Finalizing WidgetController: {GetType().Name}");
}