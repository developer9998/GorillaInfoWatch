using System;

namespace GorillaInfoWatch.Models;

[Flags]
public enum LineRestrictions
{
    None = 1 << 0,
    Wrapping = 1 << 1
}
