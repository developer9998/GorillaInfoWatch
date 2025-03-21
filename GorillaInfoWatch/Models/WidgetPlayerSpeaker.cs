using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models
{
    public class WidgetPlayerSpeaker(NetPlayer player) : WidgetSymbol(new Symbol(null)), IWidgetBehaviour
    {
        public NetPlayer Player = player;

        public GameObject game_object { get; set; }

        public bool PerformNativeMethods => false;

        private RigContainer rigContainer;

        private VRRig rig;

        private Image image;

        private Recorder recorder;

        private Sprite open_speaker, muted_speaker;

        public void Initialize(GameObject gameObject)
        {
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.OpenSpeaker, out open_speaker);
            Main.Instance.Sprites.TryGetValue(EDefaultSymbol.MutedSpeaker, out muted_speaker);

            recorder = NetworkSystem.Instance.LocalRecorder;

            if (RigUtils.TryGetVRRig(Player, out rigContainer) && (bool)rigContainer.Rig)
            {
                rig = rigContainer.Rig;
                image = gameObject.GetComponent<Image>();
                image.enabled = true;
                if (!image.GetComponent<LayoutElement>())
                {
                    var element = image.gameObject.AddComponent<LayoutElement>();
                    element.ignoreLayout = true;
                    var rect_tform = image.GetComponent<RectTransform>();
                    rect_tform.anchoredPosition3D = rect_tform.anchoredPosition3D.WithX(625).WithY(31.25f);
                    rect_tform.sizeDelta *= 0.9f;
                }
                image.enabled = false;
                Logging.Info($"PlayerSpeaker for {Player.NickName}: {((bool)rig ? rig.name : "null")}");
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
                image.enabled = false;
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