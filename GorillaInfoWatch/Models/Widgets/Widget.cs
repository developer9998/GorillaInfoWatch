using GorillaInfoWatch.Behaviours;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public abstract class Widget
    {
        public abstract EWidgetType WidgetType { get; }

        public virtual bool AllowModification { get; } = true;

        public GameObject gameObject;

        public WidgetBehaviour<Widget> behaviour;

        public virtual bool Equals(Widget widget)
        {
            return false;
        }

        public virtual void CreateObject(MenuLine menuLine)
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
