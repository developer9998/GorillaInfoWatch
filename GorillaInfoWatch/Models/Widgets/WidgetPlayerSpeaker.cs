using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetPlayerSpeaker(NetPlayer player, float offset = 620, int scaleX = 100, int scaleY = 100) : WidgetSymbol(new Symbol(null))
    {
        public override bool AllowModification => false;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        private Recorder recorder;
        private Sprite open_speaker, muted_speaker, force_mute_speaker;
        private bool is_mute_manual;

        public override bool Init()
        {
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.OpenSpeaker, out open_speaker);
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.MutedSpeaker, out muted_speaker);
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.ForceMuteSpeaker, out force_mute_speaker);

            recorder = NetworkSystem.Instance.LocalRecorder;

            if (RigUtils.TryGetVRRig(Player, out playerRig))
            {
                is_mute_manual = PlayerPrefs.HasKey(Player.UserId);
 
                image.enabled = true;

                if (image.GetComponent<LayoutElement>() is null)
                {
                    LayoutElement layoutElement = image.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;

                    RectTransform rectTransform = image.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.WithX(offset).WithY(31.25f);
                    rectTransform.sizeDelta = new Vector2(scaleX, scaleY);
                }

                image.enabled = false;

                SetSpeakerState();
            }

            return true;
        }

        public override void Update()
        {
            if (Player is not null && playerRig is not null && playerRig.Creator != Player)
            {
                Logging.Info($"PlayerSwatch for {Player.NickName} will be shut off");
                playerRig = null;
                return;
            }

            if (playerRig is not null)
                SetSpeakerState();
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

            if ((playerRig.Voice != null && playerRig.Voice.IsSpeaking) || ((playerRig.Rig.isOfflineVRRig || playerRig.Rig.Creator.IsLocal) && recorder != null && recorder.IsCurrentlyTransmitting))
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