using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_Base
    {
        public virtual float Width { get; } = 20.5f;
        public virtual float Depth { get; } = 0f;
        public virtual bool Modify { get; } = true;

        public GameObject Object;

        public WidgetAlignment Alignment = WidgetAlignment.Right;

        public Type ControllerType = null;

        public object[] ControllerParameters = null;

        internal WidgetController Controller = null;

        public virtual void Object_Construct(WatchLine menuLine)
        {

        }

        public virtual void Object_Modify()
        {

        }

        public virtual bool Equals(Widget_Base widget) => false;
    }
}
