using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using Photon.Voice.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    // anchor of 47.5
    // offset of 100
    public class WidgetController_PlayerSpeaker(NetPlayer player) : WidgetController
    {
        public override Type[] AllowedTypes => [typeof(Widget_Symbol)];
        public override bool? Modify => false;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        private Recorder recorder;
        private Sprite open_speaker, muted_speaker, force_mute_speaker;
        private bool is_mute_manual;

        private Image Image => (Widget as Widget_Symbol).image;

        public override void OnEnable()
        {
            Main.EnumToSprite.TryGetValue(Symbols.OpenSpeaker, out open_speaker);
            Main.EnumToSprite.TryGetValue(Symbols.MutedSpeaker, out muted_speaker);
            Main.EnumToSprite.TryGetValue(Symbols.ForceMuteSpeaker, out force_mute_speaker);

            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                is_mute_manual = PlayerPrefs.HasKey(Player.UserId);
                Image.enabled = false;

                SetSpeakerState();
            }
        }

        public override void Update()
        {
            base.Update();

            if (Player is not null && playerRig is not null && playerRig.Creator != Player)
            {
                Logging.Info($"PlayerSpeaker for {Player.NickName} will be shut off");
                Enabled = false;
                playerRig = null;

                return;
            }

            if (playerRig is not null) SetSpeakerState();
        }
        public void SetSpeakerState()
        {
            if (!is_mute_manual && playerRig.GetIsPlayerAutoMuted())
            {
                if (!Image.enabled || Image.sprite != force_mute_speaker)
                {
                    Image.sprite = force_mute_speaker;
                    Image.enabled = true;
                }
                return;
            }

            if (playerRig.Muted)
            {
                if (!Image.enabled || Image.sprite != muted_speaker)
                {
                    Image.sprite = muted_speaker;
                    Image.enabled = true;
                }
                return;
            }

            if (playerRig.Rig.remoteUseReplacementVoice || playerRig.Rig.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn == "FALSE")
            {
                if (playerRig.Rig.SpeakingLoudness > playerRig.Rig.replacementVoiceLoudnessThreshold && !playerRig.ForceMute && !playerRig.Muted)
                {
                    if (!Image.enabled || Image.sprite != open_speaker)
                    {
                        Image.sprite = open_speaker;
                        Image.enabled = true;
                    }
                    return;
                }

                goto HideSpeaker;
            }

            if (recorder == null) recorder = NetworkSystem.Instance.LocalRecorder; ;

            if ((playerRig.Voice != null && playerRig.Voice.IsSpeaking) || (playerRig.Rig.isLocal && recorder.IsCurrentlyTransmitting))
            {
                if (!Image.enabled || Image.sprite != open_speaker)
                {
                    Image.sprite = open_speaker;
                    Image.enabled = true;
                }
                return;
            }

        HideSpeaker:
            if (Image.enabled) Image.enabled = false;
        }
    }
}
