using System;
using System.ComponentModel;

namespace GorillaInfoWatch.Models;

[Flags]
internal enum NotificationSource
{
    None = 0,
    [Description("Includes messages sent detailing Photon authentication and room status")]
    Server = 1 << 0,
    [Description("Includes the message sent when any assigned quest has been completed")]
    Quest = 1 << 1,
    [Description("Includes messages sent when a friended user enters or leaves (GorillaFriends)")]
    Friend = 1 << 2,
    [Description("Includes messages sent when a verified user enters or leaves (GorillaFriends)")]
    Verified = 1 << 3,
    [Description("Includes messages sent when a user credited in the mod enters or leaves")]
    ModSignificant = 1 << 4,
    [Description("Includes the message sent when a user with a special cosmetic loads in")]
    CosmeticSignificant = 1 << 5,
    Significance = ModSignificant | CosmeticSignificant,
    GorillaFriends = Friend | Verified,
    All = Server | Quest | Friend | Verified | ModSignificant | CosmeticSignificant
}
