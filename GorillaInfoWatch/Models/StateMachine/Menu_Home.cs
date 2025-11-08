using GorillaInfoWatch.Behaviours;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_Home(Watch watch) : Menu_StateBase(watch)
    {
        private int frameCount;
        private float timeCount;

        public GameObject fpsObject, pingObject;
        private TMP_Text fpsText, pingText;
        private Image micIcon, bellIcon;

        private Symbol micSpeakSymbol, micMuteSymbol, micTimeoutSymbol, bellIdleSymbol, bellRingSymbol;

        private Recorder _localRecorder = null;

        private bool isLocalWatch;
        private RigContainer targetRig;

        private GorillaSpeakerLoudness _speakerLoudness;

        private MicrophoneIconState _iconState;

        public override void Enter()
        {
            base.Enter();

            Watch.homeMenu.SetActive(true);
        }

        public override void Initialize()
        {
            base.Initialize();

            isLocalWatch = Watch.LocalWatch == Watch;
            targetRig = isLocalWatch ? VRRigCache.Instance.localRig : (Watch.Rig.rigContainer ?? Watch.Rig.GetComponent<RigContainer>());

            fpsObject = Watch.homeMenu.transform.Find("BottomLeftCorner/FPS").gameObject;
            fpsText = fpsObject.transform.GetChild(0).GetComponent<TMP_Text>();
            fpsObject.SetActive(true);

            pingObject = Watch.homeMenu.transform.Find("BottomLeftCorner/Ping").gameObject;
            pingText = pingObject.transform.GetChild(0).GetComponent<TMP_Text>();
            pingObject.SetActive(isLocalWatch);

            micIcon = Watch.homeMenu.transform.Find("BottomRightCorner/Speaker").GetComponent<Image>();
            bellIcon = Watch.homeMenu.transform.Find("BottomRightCorner/Bell").GetComponent<Image>();
            bellIcon.enabled = isLocalWatch;

            micSpeakSymbol = Symbol.GetSharedSymbol(Symbols.OpenSpeaker);
            micMuteSymbol = Symbol.GetSharedSymbol(Symbols.MutedSpeaker);
            micTimeoutSymbol = Symbol.GetSharedSymbol(Symbols.ForceMuteSpeaker);
            bellIdleSymbol = Symbol.GetSharedSymbol(Symbols.Bell);
            bellRingSymbol = Symbol.GetSharedSymbol(Symbols.BellRing);

            InfrequentUpdate();
        }

        public override void Resume()
        {
            base.Resume();

            frameCount = 0;
            timeCount = 0;
        }

        public override void Exit()
        {
            base.Exit();

            Watch.homeMenu.SetActive(false);
        }

        public override void Update()
        {
            base.Update();

            if (timeCount < 0.5f)
            {
                timeCount += Time.unscaledDeltaTime;
                frameCount++;
            }
            else
            {
                InfrequentUpdate();
                timeCount = 0f;
                frameCount = 0;
            }

            MicrophoneIconState iconState = MicrophoneIconState.Silent;

            if (isLocalWatch)
            {
                if (GorillaTagger.moderationMutedTime > 0)
                {
                    iconState = MicrophoneIconState.MuteViaPunishment;
                    goto CheckMicIcon;
                }

                if (NetworkSystem.Instance.InRoom && _localRecorder == null) _localRecorder = NetworkSystem.Instance.LocalRecorder;

                if (_speakerLoudness == null) _speakerLoudness = GorillaTagger.Instance.offlineVRRig.mySpeakerLoudness ?? GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaSpeakerLoudness>();

                if ((_speakerLoudness != null && !_speakerLoudness.IsMicEnabled) || (_localRecorder != null && !_localRecorder.TransmitEnabled))
                {
                    iconState = MicrophoneIconState.MuteViaPreference;
                    goto CheckMicIcon;
                }

                if (_speakerLoudness != null)
                {
                    float squareRoot = Mathf.Sqrt(_speakerLoudness.LoudnessNormalized);
                    bool isSpeaking = Mathf.FloorToInt(squareRoot * 50f) >= 3;
                    iconState = isSpeaking ? MicrophoneIconState.Speaking : MicrophoneIconState.Silent;
                }
            }
            else
            {
                if (_speakerLoudness == null) _speakerLoudness = targetRig.Rig.mySpeakerLoudness ?? targetRig.Rig.GetComponent<GorillaSpeakerLoudness>();

                if ((_speakerLoudness != null && !_speakerLoudness.IsMicEnabled) || targetRig.Muted)
                {
                    iconState = targetRig.GetIsPlayerAutoMuted() ? MicrophoneIconState.MuteViaPunishment : MicrophoneIconState.MuteViaPreference;
                    goto CheckMicIcon;
                }

                bool isSpeaking = targetRig.Voice is PhotonVoiceView voice && voice && voice.IsSpeaking;
                iconState = isSpeaking ? MicrophoneIconState.Speaking : MicrophoneIconState.Silent;
            }

        CheckMicIcon:

            if (_iconState != iconState)
            {
                _iconState = iconState;
                switch (_iconState)
                {
                    case MicrophoneIconState.Silent:
                        micIcon.sprite = micSpeakSymbol.Sprite;
                        micIcon.color = Color.grey;
                        break;
                    case MicrophoneIconState.Speaking:
                        micIcon.sprite = micSpeakSymbol.Sprite;
                        micIcon.color = Color.white;
                        break;
                    case MicrophoneIconState.MuteViaPreference:
                        micIcon.sprite = micMuteSymbol.Sprite;
                        micIcon.color = Color.white;
                        break;
                    case MicrophoneIconState.MuteViaPunishment:
                        micIcon.sprite = micTimeoutSymbol.Sprite;
                        micIcon.color = Color.white;
                        break;
                }
            }
        }

        public void InfrequentUpdate()
        {
            if (Watch.TimeOffset is float timeOffset)
            {
                DateTime dateTime = DateTime.UtcNow + TimeSpan.FromMinutes(timeOffset);
                string time = dateTime.ToShortTimeString();
                string date = dateTime.ToLongDateString();
                Watch.timeText.text = string.Format("<cspace=0.1em>{0}</cspace><br><size=50%>{1}</size>", time, date);
            }

            fpsText.text = (isLocalWatch ? Mathf.FloorToInt(frameCount / timeCount) : Watch.Rig.fps).ToString();

            if (isLocalWatch)
            {
                int ping = (PhotonNetwork.NetworkingClient is LoadBalancingClient client && client.LoadBalancingPeer is LoadBalancingPeer peer) ? peer.RoundTripTime : -1;
                if (pingObject.activeSelf != (ping != -1)) pingObject.SetActive(ping != -1);
                pingText.text = ping.ToString();
            }
        }

        public void UpdateBell(int notificationCount)
        {
            bool hasUnread = notificationCount > 0;
            bellIcon.sprite = (hasUnread ? bellRingSymbol : bellIdleSymbol).Sprite;
            bellIcon.color = hasUnread ? Color.white : Color.grey;
        }

        private enum MicrophoneIconState
        {
            None = -1,
            Silent,
            Speaking,
            MuteViaPreference,
            MuteViaPunishment
        }
    }
}
