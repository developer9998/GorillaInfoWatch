using System.Collections.Generic;
using GorillaInfoWatch.Extensions;
using HarmonyLib;

namespace GorillaInfoWatch.Patches;

[HarmonyPatch(typeof(VRRig))]
[HarmonyWrapSafe]
internal class VRRigPatches
{
    // Start of validity patches
    // Used mostly to prevent our events from being called too early

    private static readonly HashSet<VRRig> netInitRigs = [];

    private static bool IsValid(VRRig vrRig) => vrRig.isOfflineVRRig || vrRig.isLocal || netInitRigs.Contains(vrRig);

    [HarmonyPatch(nameof(VRRig.NetInitialize))]
    [HarmonyPostfix]
    [HarmonyPriority(650)]
    public static void NetInitPatch(VRRig __instance) => netInitRigs.Add(__instance);

    [HarmonyPatch(nameof(VRRig.OnDisable))]
    [HarmonyPostfix]
    [HarmonyPriority(650)]
    public static void DisablePatch(VRRig __instance) => netInitRigs.Remove(__instance);

    // End of validity patches

    [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics")]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPostfix]
    public static void GetCosmeticsPatch(VRRig __instance)
    {
        if (!IsValid(__instance)) return;
        Events.OnRigRecievedCosmetics?.SafeInvoke(__instance);
    }

    [HarmonyPatch(nameof(VRRig.UpdateName), typeof(bool))]
    [HarmonyPostfix]
    [HarmonyPriority(150)]
    public static void UpdateNamePatch(VRRig __instance)
    {
        if (!IsValid(__instance)) return;
        Events.OnRigNameUpdate?.SafeInvoke(__instance);
    }

    [HarmonyPatch(nameof(VRRig.SetInvisibleToLocalPlayer))]
    [HarmonyPostfix]
    public static void LocalInvisiblePatch(VRRig __instance, bool invisible)
    {
        if (!IsValid(__instance)) return;
        Events.OnRigSetInvisibleToLocal?.SafeInvoke(__instance, invisible);
    }
}