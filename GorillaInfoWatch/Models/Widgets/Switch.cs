using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Widgets;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Switch(WidgetCommand command = null, params object[] parameters) : Widget
    {
        public override EWidgetType WidgetType => EWidgetType.Interaction;

        public bool Value;

        public WidgetCommand Command = command;

        public object[] Parameters = parameters;

        public bool ReadOnly = false;

        public override void CreateObject(MenuLine menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.Switch.gameObject, menuLine.Switch.transform.parent);
            gameObject.name = "Switch";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out SwitchComponent component))
                component.SetWidget(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not PushButton widgetButton)
                return false;

            return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method);
        }
    }
}
