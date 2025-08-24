using GorillaInfoWatch.Extensions;
using HarmonyLib;
using System.Collections.Generic;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(VRRig)), HarmonyWrapSafe]
    internal class VRRigPatches
    {
        // Start of validity stuff

        private static readonly HashSet<VRRig> netInitRigs = [];

        private static bool IsValid(VRRig vrRig) => vrRig.isLocal || netInitRigs.Contains(vrRig);

        [HarmonyPatch(nameof(VRRig.NetInitialize)), HarmonyPostfix, HarmonyPriority(601)]
        public static void NetInitPatch(VRRig __instance)
        {
            netInitRigs.Add(__instance);
        }

        [HarmonyPatch(nameof(VRRig.OnDisable)), HarmonyPostfix, HarmonyPriority(601)]
        public static void DisablePatch(VRRig __instance)
        {
            netInitRigs.Remove(__instance);
        }

        // End of validity stuff

        [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics"), HarmonyPostfix, HarmonyPriority(Priority.Low), HarmonyWrapSafe]
        public static void GetCosmeticsPatch(VRRig __instance)
        {
            if (IsValid(__instance)) Events.OnRigRecievedCosmetics?.SafeInvoke(__instance);
        }

        [HarmonyPatch(nameof(VRRig.UpdateName), typeof(bool)), HarmonyPostfix, HarmonyPriority(Priority.Low), HarmonyWrapSafe]
        public static void UpdateNamePatch(VRRig __instance)
        {
            if (IsValid(__instance)) Events.OnRigNameUpdate?.SafeInvoke(__instance);
        }

        [HarmonyPatch(nameof(VRRig.SetInvisibleToLocalPlayer)), HarmonyPostfix]
        public static void LocalInvisiblePatch(VRRig __instance, bool invisible)
        {
            if (IsValid(__instance)) Events.OnRigSetInvisibleToLocal?.SafeInvoke(__instance, invisible);
        }
    }
}
