using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Widgets;

namespace GorillaInfoWatch.Models.Widgets
{
    public class SnapSlider(WidgetCommand command = null, int value = 0, int start = 0, int end = 10) : Widget
    {
        public override EWidgetType WidgetType => EWidgetType.Interaction;

        public int Value = value;

        public WidgetCommand Command = command;

        public int StartValue = start;

        public int EndValue = end;

        public bool ReadOnly;

        public override void CreateObject(MenuLine menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.SnapSlider.gameObject, menuLine.SnapSlider.transform.parent);

            gameObject.name = "SnapSlider";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out SnapSliderComponent component))
                component.ApplySlider(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not SnapSlider widgetSnapSlider)
                return false;

            return Command.Target == widgetSnapSlider.Command.Target && Command.Method.Equals(widgetSnapSlider.Command.Method) && StartValue == widgetSnapSlider.StartValue && EndValue == widgetSnapSlider.EndValue;
        }
    }
}