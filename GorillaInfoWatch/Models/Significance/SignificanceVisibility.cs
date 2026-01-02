using System;

namespace GorillaInfoWatch.Models.Significance;

[Flags]
public enum SignificanceVisibility
{
    None = 0,
    Item = 1 << 0,
    Figure = 1 << 1,
    All = Item | Figure
}
