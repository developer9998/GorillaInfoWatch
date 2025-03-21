using GorillaInfoWatch.Behaviours;
using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(RigContainer), "OnDisable")]
    public class RigDisablePatch
    {
        [HarmonyWrapSafe]
        public static void Postfix(RigContainer __instance)
        {
            /*VRRig rig = __instance.Rig;
            Events.Instance.PlayerLeft(__instance.Creator, rig);*/
            if (__instance.TryGetComponent(out WatchOwner watch_owner))
            {
                UnityEngine.Object.Destroy(watch_owner);
            }
        }
    }
}