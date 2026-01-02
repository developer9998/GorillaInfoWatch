using GorillaGameModes;
using GorillaInfoWatch.Extensions;
using HarmonyLib;
using Photon.Pun;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch]
    public class GameModePatches
    {
        [HarmonyPatch(typeof(GameMode), nameof(GameMode.BroadcastTag)), HarmonyPostfix]
        public static void MasterTagPatch(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
        {
            if (!NetworkSystem.Instance.IsMasterClient || GameMode.activeNetworkHandler is not GameModeSerializer handler || handler.Null()) return;
            Events.OnPlayerTagged?.SafeInvoke(handler.gameModeInstance, taggedPlayer, taggingPlayer);
        }

        [HarmonyPatch(typeof(GameMode), nameof(GameMode.BroadcastRoundComplete)), HarmonyPostfix]
        public static void MasterRoundCompletePatch()
        {
            if (!NetworkSystem.Instance.IsMasterClient || GameMode.activeNetworkHandler is not GameModeSerializer handler || handler.Null()) return;
            Events.OnRoundComplete?.SafeInvoke(handler.gameModeInstance);
        }

        [HarmonyPatch(typeof(GameModeSerializer), nameof(GameModeSerializer.BroadcastTag), argumentTypes: [typeof(NetPlayer), typeof(NetPlayer), typeof(PhotonMessageInfo)]), HarmonyPostfix]
        public static void ClientTagPatch(GameModeSerializer __instance, NetPlayer taggedPlayer, NetPlayer taggingPlayer, PhotonMessageInfo info)
        {
            if (NetworkSystem.Instance.IsMasterClient || taggedPlayer == null || taggingPlayer == null || !info.Sender.IsMasterClient) return;
            Events.OnPlayerTagged?.SafeInvoke(__instance.gameModeInstance, taggedPlayer, taggingPlayer);
        }

        [HarmonyPatch(typeof(GameModeSerializer), nameof(GameModeSerializer.BroadcastRoundComplete), argumentTypes: [typeof(PhotonMessageInfoWrapped)]), HarmonyPostfix]
        public static void ClientRoundCompletePatch(GameModeSerializer __instance, PhotonMessageInfoWrapped info)
        {
            if (NetworkSystem.Instance.IsMasterClient || !info.Sender.IsMasterClient) return;
            Events.OnRoundComplete?.SafeInvoke(__instance.gameModeInstance);
        }
    }
}