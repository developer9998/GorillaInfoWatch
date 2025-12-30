using System;

namespace GorillaInfoWatch.Models.Significance;

[Flags]
public enum SignificanceCheckScope
{
    None = 0,
    Figure = 1 << 0,
    Item = 1 << 1,
    Friend = 1 << 2,
    Verified = 1 << 3,
    InfoWatch = 1 << 4,
    RemovalCandidate = 1 << 5,
    PlayerJoined = Figure | Friend | Verified,
    PlayerLeft = RemovalCandidate,
    LocalPlayer = Figure | Item | Verified | InfoWatch
}
