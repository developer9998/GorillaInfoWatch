using GorillaInfoWatch.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using EffectType = GorillaTag.Cosmetics.CosmeticEffectsOnPlayers.EFFECTTYPE;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(VRRig)), HarmonyWrapSafe]
    internal class VRRigPatches
    {
        private static readonly HashSet<VRRig> initializedRigSet = [];

        [HarmonyPatch(nameof(VRRig.NetInitialize)), HarmonyPostfix, HarmonyPriority(650)]
        public static void NetInitPatch(VRRig __instance) => initializedRigSet.Add(__instance);

        [HarmonyPatch(nameof(VRRig.OnDisable)), HarmonyPrefix]
        private static void PreDisablePatch(VRRig __instance) => __instance.TemporaryCosmeticEffects.Where(effect => effect.Key == EffectType.Skin).ForEach(__instance.RemoveTemporaryCosmeticEffects);

        [HarmonyPatch(nameof(VRRig.OnDisable)), HarmonyPostfix, HarmonyPriority(650)]
        private static void PostDisablePatch(VRRig __instance) => initializedRigSet.Remove(__instance);

        [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics"), HarmonyPriority(Priority.Low), HarmonyPostfix]
        private static void GetCosmeticsPatch(VRRig __instance)
        {
            if (!IsValid(__instance)) return;
            Events.OnRigRecievedCosmetics?.SafeInvoke(__instance);
        }

        [HarmonyPatch(nameof(VRRig.UpdateName), typeof(bool)), HarmonyPostfix, HarmonyPriority(150)]
        private static void UpdateNamePatch(VRRig __instance)
        {
            if (!IsValid(__instance)) return;
            Events.OnRigNameUpdate?.SafeInvoke(__instance);
        }

        [HarmonyPatch(nameof(VRRig.SetInvisibleToLocalPlayer)), HarmonyPostfix]
        private static void LocalInvisiblePatch(VRRig __instance, bool invisible)
        {
            if (!IsValid(__instance)) return;
            Events.OnRigSetInvisibleToLocal?.SafeInvoke(__instance, invisible);
        }

        private static bool IsValid(VRRig playerRig) => playerRig.isOfflineVRRig || playerRig.isLocal || initializedRigSet.Contains(playerRig);
    }
}
