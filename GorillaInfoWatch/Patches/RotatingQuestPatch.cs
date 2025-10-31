using GorillaInfoWatch.Extensions;
using HarmonyLib;

namespace GorillaInfoWatch.Patches;

[HarmonyPatch(typeof(RotatingQuest), nameof(RotatingQuest.Complete))]
[HarmonyPriority(Priority.HigherThanNormal)]
public class RotatingQuestPatch
{
    public static void Prefix(RotatingQuest __instance)
    {
        if (__instance.isQuestComplete) return;
        Events.OnQuestCompleted?.SafeInvoke(__instance);
    }
}