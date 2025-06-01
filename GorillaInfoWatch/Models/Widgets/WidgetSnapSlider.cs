using System;
using GorillaInfoWatch.Behaviours;
using SnapSlider = GorillaInfoWatch.Behaviours.Widgets.SnapSlider;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetSnapSlider(int startValue = 0, int endValue = 100, int currentValue = 0) : Widget, IWidgetValue<int>
    {
        public override EWidgetType WidgetType => EWidgetType.Interaction;

        public int Value { get; set; } = currentValue;
        public object[] Parameters { get; set; }
        public Action<int, object[]> Command { get; set; }

        public int StartValue = startValue;

        public int EndValue = endValue;

        public override void CreateObject(MenuLine menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.SnapSlider.gameObject, menuLine.SnapSlider.transform.parent);

            gameObject.name = "SnapSlider";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out SnapSlider snapSlider))
                snapSlider.ApplySlider(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not WidgetSnapSlider widgetSnapSlider)
                return false;

            return Command.Target == widgetSnapSlider.Command.Target && Command.Method.Equals(widgetSnapSlider.Command.Method) && StartValue == widgetSnapSlider.StartValue && EndValue == widgetSnapSlider.EndValue;
        }
    }
}