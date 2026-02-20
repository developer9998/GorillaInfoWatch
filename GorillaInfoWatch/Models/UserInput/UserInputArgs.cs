using System;

namespace GorillaInfoWatch.Models.UserInput;

public class UserInputArgs : EventArgs
{
    public string Input { get; set; }
    public bool IsTyping { get; set; }
}