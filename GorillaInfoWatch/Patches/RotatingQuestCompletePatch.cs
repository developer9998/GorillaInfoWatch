using GorillaInfoWatch.Extensions;
using HarmonyLib;
using static RotatingQuestsManager;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(RotatingQuest), nameof(RotatingQuest.Complete)), HarmonyWrapSafe]
    public class RotatingQuestCompletePatch
    {
        public static void Prefix(RotatingQuest __instance)
        {
            if (__instance.isQuestComplete)
                return;

            Events.OnCompleteQuest?.SafeInvoke(__instance);
        }
    }
}
