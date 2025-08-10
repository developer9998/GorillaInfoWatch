using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_Home(InfoWatch watch) : Menu_StateBase(watch)
    {
        public GameObject fpsObject;

        private int frameCount;
        private float timeCount;

        private TMP_Text fpsText;
        private Image micIcon, bellIcon;

        private Symbol micSpeakSymbol, micMuteSymbol, micTimeoutSymbol, bellIdleSymbol, bellRingSymbol;

        private Recorder recorder = null;

        private bool isLocalWatch;
        private RigContainer targetRig;

        public override void Enter()
        {
            base.Enter();

            Watch.idleMenu.SetActive(true);

            if (Watch.Rig.isOfflineVRRig)
            {
                NetworkSystem.Instance.OnMultiplayerStarted += RefreshMic;
                NetworkSystem.Instance.OnReturnedToSinglePlayer += RefreshMic;
            }

            isLocalWatch = InfoWatch.LocalWatch == Watch;
            targetRig = isLocalWatch ? VRRigCache.Instance.localRig : (Watch.Rig.rigContainer ?? Watch.Rig.GetComponent<RigContainer>());
        }

        public override void Initialize()
        {
            base.Initialize();

            fpsObject = Watch.idleMenu.transform.Find("FPS").gameObject;
            fpsText = fpsObject.transform.GetChild(0).GetComponent<TMP_Text>();
            fpsObject.SetActive(true);

            micIcon = Watch.idleMenu.transform.Find("Speaker").GetComponent<Image>();
            bellIcon = Watch.idleMenu.transform.Find("Bell").GetComponent<Image>();
            bellIcon.enabled = Watch.Rig.isOfflineVRRig;

            micSpeakSymbol = Symbols.OpenSpeaker;
            micMuteSymbol = Symbols.MutedSpeaker;
            micTimeoutSymbol = Symbols.ForceMuteSpeaker;
            bellIdleSymbol = Symbols.Bell;
            bellRingSymbol = Symbols.BellRing;
        }

        public override void Resume()
        {
            base.Resume();

            frameCount = 0;
            timeCount = 0;

            RefreshMic();
        }

        public override void Exit()
        {
            base.Exit();

            Watch.idleMenu.SetActive(false);

            if (Watch.Rig.isLocal)
            {
                NetworkSystem.Instance.OnMultiplayerStarted -= RefreshMic;
                NetworkSystem.Instance.OnReturnedToSinglePlayer -= RefreshMic;
            }
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

            if (NetworkSystem.Instance.InRoom)
            {
                try
                {
                    if (!micIcon.enabled) micIcon.enabled = true;

                    if (isLocalWatch && recorder == null)
                    {
                        recorder = NetworkSystem.Instance.LocalRecorder;
                    }

                    if (isLocalWatch && GorillaTagger.moderationMutedTime > 0)
                    {
                        if (micIcon.sprite != micTimeoutSymbol.Sprite)
                        {
                            micIcon.sprite = micTimeoutSymbol.Sprite;
                            micIcon.color = Color.white;
                        }
                        return;
                    }

                    if (isLocalWatch ? (!targetRig.Rig.mySpeakerLoudness.IsMicEnabled || !recorder.TransmitEnabled) : (targetRig.Muted || !targetRig.Rig.mySpeakerLoudness.IsMicEnabled))
                    {
                        if (micIcon.sprite != micMuteSymbol.Sprite)
                        {
                            micIcon.sprite = micMuteSymbol.Sprite;
                            micIcon.color = Color.white;
                        }
                        return;
                    }

                    bool isSpeaking = isLocalWatch ? (recorder.IsCurrentlyTransmitting) : (targetRig.Voice is PhotonVoiceView voice && voice.IsSpeaking);
                    float value = isSpeaking ? 1f : 0.5f;

                    if (micIcon.sprite != micSpeakSymbol.Sprite) micIcon.sprite = micSpeakSymbol.Sprite;
                    if (micIcon.color.r != value) micIcon.color = new Color(value, value, value);
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }

                return;
            }

            recorder = null;
            if (micIcon.enabled) micIcon.enabled = false;
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
        }

        public void RefreshBell(int notificationCount)
        {
            bool hasUnread = notificationCount > 0;
            float floatingPoint = hasUnread ? 1f : 0.2f;
            Symbol symbol = hasUnread ? bellRingSymbol : bellIdleSymbol;
            bellIcon.sprite = symbol.Sprite;
            bellIcon.color = new Color(1f, 1f, 1f, floatingPoint);
        }

        public void RefreshMic()
        {
            if (micIcon) micIcon.enabled = PhotonNetwork.InRoom;
        }
    }
}
