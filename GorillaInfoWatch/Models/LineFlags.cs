using System;

namespace GorillaInfoWatch.Models
{
    [Flags]
    public enum LineFlags
    {
        None = 0,
        ActiveSpeakerSymbol = 1 << 0,
        MutedSpeakerSymbol = 1 << 1
    }
}
