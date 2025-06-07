using System;
using static RotatingQuestsManager;

namespace GorillaInfoWatch
{
    public class Events
    {
        public static Events Instance = new();

        public static Action<VRRig, bool> OnSetInvisibleToLocalPlayer;

        public static Action<RotatingQuest> OnCompleteQuest;
    }
}