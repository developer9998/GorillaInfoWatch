using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using System;
using UnityEngine;

namespace GorillaInfoWatch.PlayerFunctions
{
    public class Volume : IPlayerFunction
    {
        public Action<PlayerInfo> OnPlayerJoin => (PlayerInfo Arguments) =>
        {
            if (Arguments.VoiceView)
            {
                AudioSource Speaker = Arguments.VoiceView.SpeakerInUse.GetComponent<AudioSource>();
                Speaker.volume = DataManager.GetItem(string.Concat(Arguments.Player.UserId, "_volume"), 1f, DataType.Stored);
            }
        };

        public Action<PlayerInfo> OnPlayerLeave => (PlayerInfo Arguments) =>
        {
            if (Arguments.VoiceView)
            {
                AudioSource Speaker = Arguments.VoiceView.SpeakerInUse.GetComponent<AudioSource>();
                Speaker.volume = 1f;
            }
        };
    }
}
