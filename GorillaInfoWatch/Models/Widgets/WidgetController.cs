using System;

namespace GorillaInfoWatch.Models.Widgets;

public abstract class WidgetController
{
    public abstract Type[] AllowedTypes { get; }

    public Widget_Base Widget { get; set; }
    public virtual float? Depth { get; }
    public virtual bool? Modification { get; }

    public bool Enabled = true;

    public virtual void OnEnable()
    {

    }

    public virtual void OnDisable()
    {

    }

    public virtual void Update()
    {

    }

    // ~WidgetController() => Logging.Message($"Finalizing WidgetController: {GetType().Name}");
}
