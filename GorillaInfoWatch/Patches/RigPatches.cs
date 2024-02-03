using HarmonyLib;
using Photon.Realtime;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch]
    public class RigPatches
    {
        private static Events Events;

        public static void Prepare()
            => Events = new Events();

        public static void AddPatch(Player player, VRRig vrrig)
            => Events.TriggerRigAdded(player, vrrig);

        public static void RemovePatch(Player player, VRRig vrrig)
            => Events.TriggerRigRemoved(player, vrrig);
    }
}
