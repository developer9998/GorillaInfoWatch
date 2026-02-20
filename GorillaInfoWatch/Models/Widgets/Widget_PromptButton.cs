using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Behaviours.UI.Widgets;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.UserInput;
using System;

namespace GorillaInfoWatch.Models.Widgets;

public sealed class Widget_PromptButton(string input, int limit, UserInputBoard board, EventHandler<UserInputArgs> submit) : Widget_Base
{
    public string Input = input;

    public UserInputBoard Keyboard = board;

    public int Limit = limit;

    public EventHandler<UserInputArgs> Submit = submit;

    public Widget_PromptButton(string input, int limit, EventHandler<UserInputArgs> submit) : this(input, limit, UserInputBoard.Standard, submit)
    {
        // this requires a body
    }

    public Widget_PromptButton(string input, EventHandler<UserInputArgs> submit) : this(input, int.MaxValue, UserInputBoard.Standard, submit)
    {
        // this requires a body
    }

    public override void Initialize(PanelLine menuLine)
    {
        if (Object.Null())
        {
            Object = UnityEngine.Object.Instantiate(menuLine.PromptButton.gameObject, menuLine.PromptButton.transform.parent);
            Object.name = "Prompt Button";
            Object.SetActive(true);
        }
    }

    public override void Modify()
    {
        if (Object.Exists() && Object.TryGetComponent(out PromptButton component)) component.Widget = this;
    }

    public override bool Equals(Widget_Base widget) => true;
}
