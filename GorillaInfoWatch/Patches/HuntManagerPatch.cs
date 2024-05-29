using HarmonyLib;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(GorillaHuntManager), nameof(GorillaHuntManager.StartPlaying))]
    public class HuntManagerPatch
    {
        public static void Postfix()
        {
            GorillaTagger.Instance.offlineVRRig.huntComputer.gameObject.SetActive(false);
        }
    }
}
