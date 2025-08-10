using GorillaInfoWatch.Behaviours.Networking;
using HarmonyLib;
using UnityEngine;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(RigContainer)), HarmonyWrapSafe]
    internal class RigContainerPatches
    {
        [HarmonyPatch("set_Creator")] // set_Creator: setter method for Creator property
        [HarmonyPostfix]
        public static void CreatorPatch(RigContainer __instance, NetPlayer value)
        {
            if (__instance.GetComponent<NetworkedPlayer>()) return;

            NetworkedPlayer networkComponent = __instance.gameObject.AddComponent<NetworkedPlayer>();
            networkComponent.Rig = __instance.Rig;
            networkComponent.Owner = value;
        }

        [HarmonyPatch(nameof(RigContainer.OnDisable))]
        [HarmonyPostfix]
        public static void DisablePatch(RigContainer __instance)
        {
            if (__instance.TryGetComponent(out NetworkedPlayer networkComponent))
            {
                Object.Destroy(networkComponent);
            }
        }
    }
}
