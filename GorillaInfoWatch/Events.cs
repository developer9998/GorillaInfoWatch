using System;

namespace GorillaInfoWatch
{
    public class Events
    {
        public static Events Instance = new();

        public static Action<VRRig, bool> OnSetInvisibleToLocalPlayer;
    }
}