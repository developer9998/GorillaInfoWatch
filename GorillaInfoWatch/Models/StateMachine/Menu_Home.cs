﻿using GorillaInfoWatch.Behaviours;
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

        private Recorder recorder;

        bool micEnabled;
        Symbol micSymbol = null;
        float? micFloatingPoint = null;

        public override void Enter()
        {
            base.Enter();

            Watch.idleMenu.SetActive(true);

            if (Watch.Rig.isOfflineVRRig)
            {
                NetworkSystem.Instance.OnMultiplayerStarted += RefreshMic;
                NetworkSystem.Instance.OnReturnedToSinglePlayer += RefreshMic;
            }
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

            micSpeakSymbol = InfoWatchSymbol.OpenSpeaker;
            micMuteSymbol = InfoWatchSymbol.MutedSpeaker;
            micTimeoutSymbol = InfoWatchSymbol.ForceMuteSpeaker;
            bellIdleSymbol = InfoWatchSymbol.Bell;
            bellRingSymbol = InfoWatchSymbol.BellRing;
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
                    if (recorder is null)
                    {
                        recorder = NetworkSystem.Instance.LocalRecorder;
                    }

                    if (Watch.Rig.isOfflineVRRig)
                    {
                        micEnabled = Watch.Rig.mySpeakerLoudness.IsMicEnabled && recorder != null && recorder.TransmitEnabled;
                        micSymbol = GorillaTagger.moderationMutedTime > 0 ? micTimeoutSymbol : (micEnabled ? micSpeakSymbol : micMuteSymbol);
                        micFloatingPoint = (!micEnabled || (recorder != null && recorder.IsCurrentlyTransmitting)) ? 1f : 0.2f;
                    }
                    else if (!Watch.Rig.isOfflineVRRig && Watch.Rig.rigContainer is RigContainer playerRig)
                    {
                        micEnabled = Watch.Rig.mySpeakerLoudness.IsMicEnabled;
                        micSymbol = playerRig.GetIsPlayerAutoMuted() ? micTimeoutSymbol : (micEnabled ? micSpeakSymbol : micMuteSymbol);
                        micFloatingPoint = (!micEnabled || (playerRig.Voice is PhotonVoiceView voice && voice.IsSpeaking)) ? 1f : 0.2f;
                    }

                    if (micSymbol != null && micIcon.sprite != micSymbol.Sprite) micIcon.sprite = micSymbol.Sprite;
                    if (micFloatingPoint != null && micIcon.color.a != micFloatingPoint) micIcon.color = new Color(1f, 1f, 1f, micFloatingPoint.GetValueOrDefault(1f));
                }
                catch (Exception)
                {

                }

                return;
            }

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

            fpsText.text = (Watch.Rig.isOfflineVRRig ? Mathf.FloorToInt(frameCount / timeCount) : Watch.Rig.fps).ToString();
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
