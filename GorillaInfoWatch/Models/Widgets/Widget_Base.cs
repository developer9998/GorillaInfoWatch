using GorillaInfoWatch.Behaviours.UI;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public abstract class Widget_Base
    {
        public virtual bool AllowModification { get; } = true;
        public virtual bool UseBehaviour { get; } = false;

        public bool Enabled = true;

        public GameObject gameObject;

        public virtual void Object_Construct(WatchLine menuLine)
        {

        }

        public virtual void Object_Modify()
        {

        }

        public virtual void Behaviour_Enable()
        {

        }

        public virtual void Behaviour_Disable()
        {

        }

        public virtual void Behaviour_Update()
        {

        }

        public virtual void Behaviour_FixedUpdate()
        {

        }

        public virtual bool Equals(Widget_Base widget)
        {
            return false;
        }
    }
}
