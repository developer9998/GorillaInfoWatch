using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public enum InfoWatchSymbol // TIP: use .meta file for looking at names for sprite sheet without having to open unity
    {
        // People
        [Tooltip("Creator + Developer for GorillaInfoWatch")]
        Dev,
        [Tooltip("Creator + Developer for GorillaInfoWatch")]
        Gizmo,
        [Tooltip("2D Artist for GorillaInfoWatch")]
        Cresmondo,
        [Tooltip("Evil")]
        BloodJman,
        [Tooltip("3D Artist for GorillaInfoWatch")]
        H4RNS,
        [Tooltip("Tester for GorillaInfoWatch")]
        Astrid,
        [Tooltip("Tester for GorillaInfoWatch")]
        Deactivated,
        [Tooltip("Tester for GorillaInfoWatch")]
        Cyan,
        [Tooltip("Friend, creator of the Grate menu")] // "grate is a panel" i then proceeded to shoot you 57 times
        Graze,
        [Tooltip("Friend")]
        Chucken,
        [Tooltip("Friend")]
        Socks,
        [Tooltip("Tester for GorillaInfoWatch")]
        Will,
        [Tooltip("Tester for GorillaInfoWatch")]
        Lapis,
        [Tooltip("Tester for GorillaInfoWatch")]
        Kronicahl,
        // Icons
        [Tooltip("Light green circle with green checkmark")]
        Verified,
        [Tooltip("Light blue circle with white \"i\" symbol")]
        Info,
        [Tooltip("Light orange circle with Patreon logo")]
        Patreon,
        [Tooltip("Cyan circle with Ko-fi logo")]
        KoFi,
        [Tooltip("White speaker with sound waves")]
        OpenSpeaker,
        [Tooltip("Red speaker with X")]
        MutedSpeaker,
        [Tooltip("Orange speaker with X")]
        ForceMuteSpeaker,
        [Tooltip("Moderator stick")]
        ModStick,
        [Tooltip("Finger Painter Badge")]
        FingerPainter,
        [Tooltip("Illustrator")]
        Illustrator,
        [Tooltip("Forest Guide stick")]
        ForestGuideStick,
        [Tooltip("The info watch")]
        InfoWatch,
        [Tooltip("A blank gorilla head")]
        TemplateHead,
        [Tooltip("A gorilla face with ears")]
        TemplateFace,
        [Tooltip("A crossed-off eye")]
        Ignore,
        [Tooltip("A sample image/placeholder")]
        Image,
        Bell,
        BellRing,
        RedFlag,
        GreenFlag,
        [Tooltip("A grey croptop on a black gorilla")]
        Croptop,
        Rainbow,
        LightBulb,
        Camera,
        [Tooltip("A gorilla hand with movement lines")]
        Vibration,
        Gun,
        [Tooltip("A set of white lines forming a circle")]
        Invisibility,
        [Tooltip("A set of blue droplets")]
        Particles,
        Phone,
        [Tooltip("A green rectangle with a play symbol")]
        Play,
        [Tooltip("A red rectangle with a stop symbol")]
        Stop
    }
}