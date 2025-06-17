using UnityEngine.Serialization;

namespace GorillaInfoWatch.Models
{
    public enum InfoWatchSymbol // TIP: use .meta file for looking at names for sprite sheet without having to open unity
    {
        // People
        Dev,
        Gizmo,
        Cresmondo,
        BloodJman,
        H4RNS,
        Astrid,
        Deactivated,
        Cyan,
        Graze,
        Chucken,
        Socks,
        Will,
        Lapis,
        Kronicahl,
        // Icons
        Verified,
        Patreon,
        KoFi,
        OpenSpeaker,
        MutedSpeaker,
        ForceMuteSpeaker,
        [FormerlySerializedAs("Stick")]
        ModStick,
        FingerPainter,
        Illustrator,
        ForestGuideStick,
        InfoWatch
    }
}