using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Widgets;

namespace GorillaInfoWatch.Models.Widgets
{
    public class PushButton(WidgetCommand command, params object[] parameters) : Widget
    {
        public override EWidgetType WidgetType => EWidgetType.Interaction;

        public WidgetCommand Command = command;

        public object[] Parameters = parameters;

        public override void CreateObject(MenuLine menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.Button.gameObject, menuLine.Button.transform.parent);

            gameObject.name = "Button";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out PushButtonComponent component))
                component.ApplyButton(this);
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