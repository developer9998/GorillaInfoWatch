using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_PlayerSpeaker(NetPlayer player, float offset = 620, int scaleX = 100, int scaleY = 100) : Widget_Symbol(new Symbol(null))
    {
        public override bool AllowModification => false;
        public override bool UseBehaviour => true;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        private Recorder recorder;
        private Sprite open_speaker, muted_speaker, force_mute_speaker;
        private bool is_mute_manual;

        public override void Behaviour_Enable()
        {
            Main.Sprites.TryGetValue(InfoWatchSymbol.OpenSpeaker, out open_speaker);
            Main.Sprites.TryGetValue(InfoWatchSymbol.MutedSpeaker, out muted_speaker);
            Main.Sprites.TryGetValue(InfoWatchSymbol.ForceMuteSpeaker, out force_mute_speaker);

            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                is_mute_manual = PlayerPrefs.HasKey(Player.UserId);

                image.enabled = true;

                LayoutElement layoutElement = image.gameObject.GetOrAddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                RectTransform rectTransform = image.GetComponent<RectTransform>();
                rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.WithX(offset).WithY(31.25f);
                rectTransform.sizeDelta = new Vector2(scaleX, scaleY);

                image.enabled = false;

                SetSpeakerState();
            }
        }

        public override void Behaviour_Update()
        {
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
                if (!image.enabled || image.sprite != force_mute_speaker)
                {
                    image.sprite = force_mute_speaker;
                    image.enabled = true;
                }
                return;
            }

            if (playerRig.Muted)
            {
                if (!image.enabled || image.sprite != muted_speaker)
                {
                    image.sprite = muted_speaker;
                    image.enabled = true;
                }
                return;
            }

            if (playerRig.Rig.remoteUseReplacementVoice || playerRig.Rig.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn == "FALSE")
            {
                if (playerRig.Rig.SpeakingLoudness > playerRig.Rig.replacementVoiceLoudnessThreshold && !playerRig.ForceMute && !playerRig.Muted)
                {
                    if (!image.enabled || image.sprite != open_speaker)
                    {
                        image.sprite = open_speaker;
                        image.enabled = true;
                    }
                    return;
                }

                goto HideSpeaker;
            }

            if (recorder == null) recorder = NetworkSystem.Instance.LocalRecorder; ;

            if ((playerRig.Voice != null && playerRig.Voice.IsSpeaking) || (playerRig.Rig.isLocal && recorder.IsCurrentlyTransmitting))
            {
                if (!image.enabled || image.sprite != open_speaker)
                {
                    image.sprite = open_speaker;
                    image.enabled = true;
                }
                return;
            }

        HideSpeaker:
            if (image.enabled) image.enabled = false;
        }
    }
}