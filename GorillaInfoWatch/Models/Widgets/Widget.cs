using GorillaInfoWatch.Behaviours.UI;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public abstract class Widget
    {
        public virtual bool AllowModification { get; } = true;

        public GameObject gameObject;

        public WidgetBehaviour<Widget> behaviour;

        public virtual bool Equals(Widget widget)
        {
            return false;
        }

        public virtual void CreateObject(InfoWatchLine menuLine)
        {

        }

        public virtual void ModifyObject()
        {

        }

        public virtual bool Init()
        {
            return false;
        }

        public virtual void Update()
        {

        }
    }
}
