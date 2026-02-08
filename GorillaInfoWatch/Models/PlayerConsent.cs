using System;

namespace GorillaInfoWatch.Models;

[Flags]
public enum PlayerConsent
{
    None = 0,
    Item = 1 << 0,
    Figure = 1 << 1,
    All = Item | Figure
}
