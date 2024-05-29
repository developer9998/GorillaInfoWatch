using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.EnableHuntWatch))]
    public class HuntComputerPatch
    {
        public static bool Prefix(VRRig __instance)
        {
            if (!__instance.isOfflineVRRig) return true;

            __instance.huntComputer.gameObject.SetActive(false);

            return false;
        }
    }
}
