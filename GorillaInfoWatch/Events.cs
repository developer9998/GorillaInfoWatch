using System;

namespace GorillaInfoWatch
{
    public class Events
    {
        internal static Action<VRRig> OnRigNameUpdate, OnRigRecievedCosmetics;

        internal static Action<VRRig, bool> OnRigSetInvisibleToLocal, OnRigUpdatedCosmetics;

        internal static Action<RotatingQuest> OnQuestCompleted;

        internal static Action<GorillaGameManager, NetPlayer, NetPlayer> OnPlayerTagged;

        internal static Action<GorillaGameManager> OnRoundComplete;
    }
}