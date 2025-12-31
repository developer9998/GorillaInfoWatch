using System;

namespace GorillaInfoWatch.Models;

[Flags]
public enum LineOptions
{
    None = 1 << 0,
    Wrapping = 1 << 1
}
