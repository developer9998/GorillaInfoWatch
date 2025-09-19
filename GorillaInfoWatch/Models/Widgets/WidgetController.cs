using GorillaInfoWatch.Tools;
using System;

namespace GorillaInfoWatch.Models.Widgets
{
    public abstract class WidgetController
    {
        public abstract Type[] AllowedTypes { get; }
        public Widget_Base Widget { get; set; }
        public virtual bool? Modify { get; }
        public virtual float? Depth { get; }

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

        ~WidgetController() => Logging.Message($"Finalizing controller: {GetType().Name}");
    }
}
