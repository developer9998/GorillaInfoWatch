using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Tools;
using Photon.Realtime;
using System;
using UnityEngine;

namespace GorillaInfoWatch.PlayerFunctions
{
    public class Volume : IPlayerFunction
    {
        public Action<Player, VRRig> OnPlayerJoin => (Player Player, VRRig Rig) =>
        {
            AudioSource Speaker = Rig.GetField<AudioSource>("voiceAudio");
            Speaker.volume = DataManager.GetItem(string.Concat(Player.UserId, "_volume"), 1f, Models.DataType.Stored);
        };

        public Action<Player, VRRig> OnPlayerLeave => (Player Player, VRRig Rig) =>
        {
            AudioSource Speaker = Rig.GetField<AudioSource>("voiceAudio");
            Speaker.volume = 1f;
        };
    }
}
