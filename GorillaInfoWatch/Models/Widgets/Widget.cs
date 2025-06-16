using GorillaInfoWatch.Behaviours.UI;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public abstract class Widget
    {
        public virtual bool AllowModification { get; } = true;
        public virtual bool UseBehaviour { get; } = false;

        public GameObject gameObject;

        public virtual bool Equals(Widget widget)
        {
            return false;
        }

        public virtual void Object_Construct(InfoWatchLine menuLine)
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
    }
}
