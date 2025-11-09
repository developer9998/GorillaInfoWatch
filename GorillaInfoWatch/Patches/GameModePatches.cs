using GorillaInfoWatch.Extensions;
using HarmonyLib;
using Photon.Pun;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(GameModeSerializer))]
    public class GameModePatches
    {
        [HarmonyPatch(nameof(GameModeSerializer.BroadcastTag), argumentTypes: [typeof(NetPlayer), typeof(NetPlayer), typeof(PhotonMessageInfo)])]
        [HarmonyPostfix]
        public static void TagPatch(NetPlayer taggedPlayer, NetPlayer taggingPlayer, PhotonMessageInfo info, GameModeSerializer __instance)
        {
            if (taggedPlayer == null || taggingPlayer == null || !info.Sender.IsMasterClient) return;
            Events.OnPlayerTagged?.SafeInvoke(__instance.gameModeInstance, taggedPlayer, taggingPlayer);
        }

        [HarmonyPatch(nameof(GameModeSerializer.BroadcastRoundComplete), argumentTypes: [typeof(PhotonMessageInfoWrapped)])]
        [HarmonyPostfix]
        public static void RoundCompletePatch(PhotonMessageInfoWrapped info, GameModeSerializer __instance)
        {
            if (!info.Sender.IsMasterClient) return;
            Events.OnRoundComplete?.SafeInvoke(__instance.gameModeInstance);
        }
    }
}