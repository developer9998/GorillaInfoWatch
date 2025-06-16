using HarmonyLib;
using GorillaInfoWatch.Extensions;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(VRRig), "IUserCosmeticsCallback.OnGetUserCosmetics")]
    public static class RigCosmeticsPatch
    {
        public static void Postfix(VRRig __instance)
        {
            Events.OnGetUserCosmetics?.SafeInvoke(__instance);
        }
    }
}
