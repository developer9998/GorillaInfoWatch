using System;

namespace GorillaInfoWatch
{
    public class Events
    {
        internal static Action<VRRig> OnRigRecievedCosmetics;

        internal static Action<VRRig, bool> OnRigSetInvisibleToLocal, OnRigUpdatedCosmetics;

        internal static Action<RotatingQuest> OnQuestCompleted;
    }
}