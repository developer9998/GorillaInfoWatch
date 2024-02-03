using GorillaInfoWatch.Utilities;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch]
    public class RigPatches
    {
        private static Events Events;

        public static void Prepare()
            => Events = new Events();

        public static void AddPatch(Player player, VRRig vrrig)
        {
            PhotonView photonView = RigCacheUtils.GetField<PhotonView>(player);
            PhotonVoiceView voiceView = RigCacheUtils.GetField<PhotonVoiceView>(player);

            Events.TriggerRigAdded(new(player, vrrig, photonView, voiceView));
        }

        public static void RemovePatch(Player player, VRRig vrrig)
        {
            PhotonView photonView = RigCacheUtils.GetField<PhotonView>(player);
            PhotonVoiceView voiceView = RigCacheUtils.GetField<PhotonVoiceView>(player);

            Events.TriggerRigRemoved(new(player, vrrig, photonView, voiceView));
        }

        public static void NetworkFinalizePatch(PhotonView photonView, PhotonVoiceView voiceView)
        {
            Player player = photonView.Owner;
            VRRig vrrig = RigCacheUtils.GetField<VRRig>(player);

            Events.TriggerRigAdded(new(player, vrrig, photonView, voiceView));
        }
    }
}
