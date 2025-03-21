using GorillaInfoWatch.Behaviours;
using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(RigContainer), "set_Creator")]
    public class RigSetCreatorPatch
    {
        [HarmonyWrapSafe]
        public static void Postfix(RigContainer __instance)
        {
            /*VRRig rig = __instance.Rig;
            Events.Instance.PlayerJoined(__instance.Creator, rig);*/
            if (!__instance.GetComponent<WatchOwner>())
            {
                __instance.gameObject.AddComponent<WatchOwner>();
            }
        }
    }
}