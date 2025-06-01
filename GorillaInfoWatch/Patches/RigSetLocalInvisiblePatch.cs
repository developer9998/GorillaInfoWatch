using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.SetInvisibleToLocalPlayer))]
    public static class RigSetLocalInvisiblePatch
    {
        public static void Postfix(VRRig __instance, bool invisible)
        {
            Events.OnSetInvisibleToLocalPlayer?.Invoke(__instance, invisible);
        }
    }
}
