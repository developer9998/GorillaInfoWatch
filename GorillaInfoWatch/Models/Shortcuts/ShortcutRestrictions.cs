using System;

namespace GorillaInfoWatch.Models.Shortcuts;

[Flags]
public enum ShortcutRestrictions
{
    None = 1 << 0,
    Singleplayer = 1 << 1,
    Multiplayer = 1 << 2
}