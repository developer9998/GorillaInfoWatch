using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using Photon.Realtime;
using System;
using UnityEngine;

namespace GorillaInfoWatch.PlayerFunctions
{
    public class Volume : IPlayerFunction
    {
        public Action<Player, VRRig> OnPlayerJoin => null;

        public Action<Player, VRRig> OnPlayerLeave => (Player Player, VRRig Rig) =>
        {
            AudioSource Speaker = Rig.GetField<AudioSource>("voiceAudio");
            Speaker.volume = 1f;
        };
    }
}
