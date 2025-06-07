using GorillaInfoWatch.Extensions;
using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.SetInvisibleToLocalPlayer)), HarmonyWrapSafe]
    public static class RigSetLocalInvisiblePatch
    {
        public static void Postfix(VRRig __instance, bool invisible)
        {
            Events.OnSetInvisibleToLocalPlayer?.SafeInvoke(__instance, invisible);
        }
    }
}
