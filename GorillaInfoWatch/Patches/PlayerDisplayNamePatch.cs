using GorillaInfoWatch.Behaviours;
using GorillaNetworking;
using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(PlayFabAuthenticator), nameof(PlayFabAuthenticator.GetPlayerDisplayName)), HarmonyWrapSafe, HarmonyPriority(Priority.Low)]
    internal class PlayerDisplayNamePatch
    {
        public static void Prefix()
        {
            if (NetworkSystem.Instance is NetworkSystem netSys && Main.HasInstance) Main.Instance.CheckPlayer(netSys.GetLocalPlayer());
        }
    }
}
