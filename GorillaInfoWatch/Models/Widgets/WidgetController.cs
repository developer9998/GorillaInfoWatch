using System;

namespace GorillaInfoWatch.Models.Widgets
{
    public abstract class WidgetController
    {
        public abstract Type[] AllowedTypes { get; }
        public Widget_Base Widget { get; set; }

        public virtual bool? AllowModification { get; }
        public virtual bool? UseBehaviour { get; }
        public virtual float? Transform_ZPosition { get; }

        public virtual void OnEnable()
        {

        }

        public virtual void OnDisable()
        {

        }

        public virtual void Update()
        {

        }
    }
}
