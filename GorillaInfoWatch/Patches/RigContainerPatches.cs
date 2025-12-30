using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(RigContainer)), HarmonyWrapSafe]
    internal class RigContainerPatches
    {
        [HarmonyPatch(nameof(RigContainer.Creator), MethodType.Setter), HarmonyPostfix]
        public static void CreatorPatch(RigContainer __instance, NetPlayer value)
        {
            if (__instance.GetComponent<NetworkedPlayer>()) return;

            NetworkedPlayer component = __instance.gameObject.AddComponent<NetworkedPlayer>();
            component.Rig = __instance.Rig;
            component.Player = value;
        }

        [HarmonyPatch(nameof(RigContainer.OnDisable)), HarmonyPostfix]
        public static void DisablePatch(RigContainer __instance)
        {
            if (!__instance.TryGetComponent(out NetworkedPlayer networkComponent)) return;
            networkComponent.Obliterate();
        }
    }
}
