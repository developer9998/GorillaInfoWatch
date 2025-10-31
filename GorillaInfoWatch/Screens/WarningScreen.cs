using System;
using GorillaInfoWatch.Models;

namespace GorillaInfoWatch.Screens;

internal class WarningScreen : InfoScreen
{
    public override string Title => "Warning";

    public override InfoContent GetContent() =>
            throw new NotImplementedException("Warning screen hasn't been implemented.. yet :3");
}