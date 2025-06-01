using System;
using GorillaInfoWatch.Behaviours;
using Button = GorillaInfoWatch.Behaviours.Widgets.Button;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetButton(Action<bool, object[]> command, params object[] parameters) : Widget, IWidgetValue<bool>
    {
        public override EWidgetType WidgetType => EWidgetType.Interaction;

        public bool Value { get; set; }
        public object[] Parameters { get; set; } = parameters;
        public Action<bool, object[]> Command { get; set; } = command;

        public EButtonType ButtonType = EButtonType.PressButton;

        public WidgetButton(EButtonType type, bool initial_value, Action<bool, object[]> command, params object[] parameters) : this(initial_value, command, parameters)
        {
            ButtonType = type;
        }

        public WidgetButton(EButtonType type, Action<bool, object[]> command, params object[] parameters) : this(command, parameters)
        {
            ButtonType = type;
        }

        public WidgetButton(bool initial_value, Action<bool, object[]> command, params object[] parameters) : this(command, parameters)
        {
            Value = initial_value;
        }

        public enum EButtonType
        {
            PressButton,
            Switch
        }

        public override void CreateObject(MenuLine menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.Button.gameObject, menuLine.Button.transform.parent);

            gameObject.name = "Button";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out Button button))
                button.ApplyButton(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not WidgetButton widgetButton)
                return false;

            return Command.Target == widgetButton.Command.Target && Command.Method.Equals(widgetButton.Command.Method) && ButtonType == widgetButton.ButtonType;
        }
    }
}