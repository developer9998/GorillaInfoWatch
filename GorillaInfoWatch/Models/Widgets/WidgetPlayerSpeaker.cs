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
    public class WidgetPlayerSpeaker(NetPlayer player, float offset = 620, int scaleX = 100, int scaleY = 100) : WidgetSymbol(new Symbol(null)), IWidgetBehaviour
    {
        public NetPlayer Player = player;

        public GameObject game_object { get; set; }

        public bool PerformNativeMethods => false;

        private RigContainer rigContainer;
        private VRRig rig;
        private Image image;
        private Recorder recorder;
        private Sprite open_speaker, muted_speaker, force_mute_speaker;
        private bool is_mute_manual;

        public void Initialize(GameObject gameObject)
        {
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.OpenSpeaker, out open_speaker);
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.MutedSpeaker, out muted_speaker);
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.ForceMuteSpeaker, out force_mute_speaker);

            recorder = NetworkSystem.Instance.LocalRecorder;

            if (RigUtils.TryGetVRRig(Player, out rigContainer))
            {
                is_mute_manual = PlayerPrefs.HasKey(Player.UserId);
                rig = rigContainer.Rig;
                image = gameObject.GetComponent<Image>();
                image.enabled = true;
                if (!image.GetComponent<LayoutElement>())
                {
                    var element = image.gameObject.AddComponent<LayoutElement>();
                    element.ignoreLayout = true;
                    var rect_tform = image.GetComponent<RectTransform>();
                    rect_tform.anchoredPosition3D = rect_tform.anchoredPosition3D.WithX(offset).WithY(31.25f);
                    rect_tform.sizeDelta = new Vector2(scaleX, scaleY);
                }
                image.enabled = false;
                //Logging.Info($"PlayerSpeaker for {Player.NickName}: {((bool)rig ? rig.name : "null")}");
                SetSpeakerState();
            }
        }

        public void InvokeUpdate()
        {
            if (!rig) return;

            if (!rig.isOfflineVRRig && !Utils.PlayerInRoom(Player.ActorNumber))
            {
                Logging.Info($"PlayerSpeaker for {Player.NickName} will be shut off");
                rig = null;
                Player = null;
                return;
            }

            SetSpeakerState();
        }

        public void SetSpeakerState()
        {
            if (!is_mute_manual && rigContainer.GetIsPlayerAutoMuted())
            {
                if (!image.enabled || image.sprite != force_mute_speaker)
                {
                    image.sprite = force_mute_speaker;
                    image.enabled = true;
                }
                return;
            }

            if (rigContainer.Muted)
            {
                if (!image.enabled || image.sprite != muted_speaker)
                {
                    image.sprite = muted_speaker;
                    image.enabled = true;
                }
                return;
            }

            if (rig.remoteUseReplacementVoice || rig.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn == "FALSE")
            {
                if (rig.SpeakingLoudness > rig.replacementVoiceLoudnessThreshold && !rigContainer.ForceMute && !rigContainer.Muted)
                {
                    if (!image.enabled || image.sprite != open_speaker)
                    {
                        image.sprite = open_speaker;
                        image.enabled = true;
                    }
                    return;
                }

                goto HideSpeaker; // im so lazy <3
            }

            if ((rigContainer.Voice != null && rigContainer.Voice.IsSpeaking) || ((rig.isOfflineVRRig || rigContainer.Creator.IsLocal) && recorder != null && recorder.IsCurrentlyTransmitting))
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