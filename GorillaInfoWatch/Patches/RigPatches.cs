using GorillaInfoWatch.Tools;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using UnityEngine;

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

        // I didn't want to hardcode it here but I felt like I really didn't have a choice sooo
        public static void NetworkFinalizePatch(PhotonView photonView, PhotonVoiceView voiceView)
        {
            AudioSource speaker = voiceView.SpeakerInUse.GetComponent<AudioSource>();
            speaker.volume = DataManager.GetItem(string.Concat(photonView.Owner.UserId, "_volume"), 1f, Models.DataType.Stored);
        }
    }
}
