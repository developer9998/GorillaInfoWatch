using System;
using GorillaInfoWatch.Behaviours;
using UnityEngine;
using Button = GorillaInfoWatch.Behaviours.Widgets.Button;

namespace GorillaInfoWatch.Models
{
    public class WidgetButton(Action<bool, object[]> command, params object[] parameters) : IWidget, IWidgetValue<bool>, IWidgetObject
    {
        public EWidgetType WidgetType => EWidgetType.Interaction;

        public EButtonType ButtonType = EButtonType.PressButton;

        public bool Value { get; set; }

        public object[] Parameters { get; set; } = parameters;

        public Action<bool, object[]> Command { get; set; } = command;

        public WidgetButton(EButtonType type, bool initial_value, Action<bool, object[]> command, params object[] parameters): this(initial_value, command, parameters)
        {
            ButtonType = type;
        }

        public WidgetButton(EButtonType type, Action<bool, object[]> command, params object[] parameters): this(command, parameters)
        {
            ButtonType = type;
        }

        public WidgetButton(bool initial_value, Action<bool, object[]> command, params object[] parameters): this(command, parameters)
        {
            Value = initial_value;
        }

        public enum EButtonType
        {
            PressButton,
            Switch
        }

        public int ObjectFits(GameObject game_object)
        {
            if (game_object && game_object.name == "Button" && game_object.TryGetComponent(out Button button))
            {
                return (button.Widget != null && button.Widget.Command.Target == Command.Target && button.Widget.Command.Method == Command.Method && button.Widget.ButtonType == ButtonType) ? 2 : 1;
            }

            return 1;
        }

        public void CreateObject(MenuLine line, out GameObject game_object)
        {
            game_object = UnityEngine.Object.Instantiate(line.Button.gameObject, line.Button.transform.parent);
            game_object.name = "Button";
            game_object.SetActive(true);
        }

        public void ModifyObject(GameObject game_object)
        {
            if (game_object.TryGetComponent(out Button button))
            {
                button.ApplyButton(this);
            }
        }
    }
}