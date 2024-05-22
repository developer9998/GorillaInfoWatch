using System;

namespace GorillaInfoWatch.Models
{
    [Flags]
    public enum LineSymbol
    {
        None = 0,
        Talk = 1 << 0,
        Mute = 1 << 1
    }
}
