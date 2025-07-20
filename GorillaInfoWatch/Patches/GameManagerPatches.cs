using GorillaInfoWatch.Extensions;
using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(GorillaGameManager))]
    public class GameManagerPatches
    {
        [HarmonyPatch(methodName: nameof(GorillaGameManager.HandleTagBroadcast), argumentTypes: [typeof(NetPlayer), typeof(NetPlayer)]), HarmonyPostfix, HarmonyWrapSafe]
        public static void TagPatch(GorillaGameManager __instance, NetPlayer taggedPlayer, NetPlayer taggingPlayer)
        {
            Events.OnPlayerTagged?.SafeInvoke(__instance, taggedPlayer, taggingPlayer);
        }

        [HarmonyPatch(nameof(GorillaGameManager.HandleRoundComplete)), HarmonyPostfix, HarmonyWrapSafe]
        public static void RoundCompletePatch(GorillaGameManager __instance)
        {
            Events.OnRoundComplete?.SafeInvoke(__instance);
        }
    }
}