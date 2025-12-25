using GorillaInfoWatch.Behaviours.Networking;
using HarmonyLib;
using UnityEngine;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(RigContainer)), HarmonyWrapSafe]
    internal class RigContainerPatches
    {
        [HarmonyPatch(nameof(RigContainer.Creator), MethodType.Setter), HarmonyPostfix]
        public static void CreatorPatch(RigContainer __instance, NetPlayer value)
        {
            if (__instance.GetComponent<NetworkedPlayer>()) return;

            NetworkedPlayer networkComponent = __instance.gameObject.AddComponent<NetworkedPlayer>();
            networkComponent.Rig = __instance.Rig;
            networkComponent.Player = value;
        }

        [HarmonyPatch(nameof(RigContainer.OnDisable)), HarmonyPostfix]
        public static void DisablePatch(RigContainer __instance)
        {
            if (__instance.TryGetComponent(out NetworkedPlayer networkComponent))
            {
                Object.Destroy(networkComponent);
            }
        }
    }
}
