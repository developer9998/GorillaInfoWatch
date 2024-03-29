﻿using ExitGames.Client.Photon;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaInfoWatch.Windows;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : MonoBehaviour, IInitializable
    {
        private bool Initialized;

        // Tools
        private Configuration Configuration;

        // Windows
        private HomeWindow HomeWindow;
        private WindowManager WindowManager;
        private WindowPlaceholderFactory WindowFactory;

        // Assets
        private GameObject Watch, Menu;
        private AudioClip Click;

        // Display
        private MenuInfo DisplayInfo;
        private float RefreshTime;

        // Animation
        private TweenExecution TweenExecution;
        private TweenInfo BackgroundTween;

        // Cache
        private readonly Dictionary<Type, IWindow> WindowCache = new();

        private bool MenuActive => Menu && Menu.activeSelf;
        private bool ConfigCheck => Configuration != null && Configuration.RefreshRate.Value != 0;
        private bool WindowCheck => WindowManager != null && WindowManager.Tab != null && Time.unscaledTime > (RefreshTime + (1f / Configuration.RefreshRate.Value));

        [Inject]
        public async void Construct(AssetLoader assetLoader, WindowPlaceholderFactory windowFactory, HomeWindow homeWindow, List<IEntry> entries, Configuration configuration, TweenExecution tweenExecution)
        {
            if (Initialized) return;
            Initialized = true;

            TweenExecution = tweenExecution;

            Configuration = configuration;

            WindowFactory = windowFactory;

            HomeWindow = homeWindow;
            WindowCache.Add(typeof(HomeWindow), HomeWindow);

            WindowManager = new WindowManager();
            WindowManager.OnTextChanged += SetText;
            WindowManager.OnWindowChanged += SetWindow;
            WindowManager.OnBackgroundChanged += SetBackground;

            #region Initialize assets 

            #region Watch

            Watch = Instantiate(await assetLoader.LoadAsset<GameObject>("Watch"));

            Watch.transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent, false);
            Watch.transform.localPosition = Vector3.zero;
            Watch.transform.localEulerAngles = Vector3.zero;
            Watch.transform.localScale = Vector3.one;
            Watch.AddComponent<WatchVisibility>();

            #endregion
            #region Menu

            Menu = Instantiate(await assetLoader.LoadAsset<GameObject>("Menu"));
            Menu.transform.SetParent(Watch.transform.Find("Point"));
            Menu.transform.localPosition = Vector3.zero;
            Menu.transform.localEulerAngles = Vector3.zero;
            Menu.AddComponent<MenuBase>();

            #endregion

            Click = await assetLoader.LoadAsset<AudioClip>("Click");

            #endregion

            DisplayInfo = new MenuInfo()
            {
                Text = Menu.transform.Find("Canvas/Text (Legacy)").GetComponent<Text>(),
                Background = Menu.transform.Find("Canvas/Background").GetComponent<Image>()
            };

            SetBackground(PresetUtils.Parse(configuration.MenuColour.Value));

            Transform buttonContainer = Menu.transform.Find("Buttons");
            for (int i = 0; i < buttonContainer.childCount; i++)
            {
                WatchButton button = buttonContainer.GetChild(i).gameObject.AddComponent<WatchButton>();
                button.Main = this;
                button.Type = (InputType)Enum.Parse(typeof(InputType), button.name);
            }

            TimeDisplay watchTime = Watch.AddComponent<TimeDisplay>();
            watchTime.Text = Watch.transform.Find("Hand Menu/Canvas/Time").GetComponent<Text>();
            watchTime.Config = Configuration;

            TimeDisplay menuTime = Menu.AddComponent<TimeDisplay>();
            menuTime.Text = Menu.transform.Find("Canvas/Upper Text").GetComponent<Text>();
            menuTime.Config = Configuration;

            WatchTrigger watchTrigger = Watch.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();
            watchTrigger.Menu = Menu;
            watchTrigger.Config = Configuration;
            watchTrigger.TweenExecution = TweenExecution;

            WindowManager.SetWindow(HomeWindow, null);
            HomeWindow.SetEntries(entries);
        }

        public void Initialize()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
        }

        public void FixedUpdate()
        {
            if (MenuActive && ConfigCheck && WindowCheck)
            {
                RefreshTime = Time.unscaledTime;
                WindowManager.Tab.OnScreenRefresh();
            }
        }

        public void PressButton(WatchButton button, bool isLeftHand)
        {
            if (!button || WindowManager.Tab == null) return;
            WindowManager.Tab.OnButtonPress(button.Type);

            AudioSource handSource = isLeftHand
                ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

            handSource.PlayOneShot(Click, 0.9f * Configuration.ButtonVolume.Value);
        }

        public void SetText(string text)
        {
            if (DisplayInfo != null && DisplayInfo.Text)
            {
                DisplayInfo.Text.text = text;
            }
        }

        public void SetBackground(Color colour)
        {
            colour.a = 178f / 255f;
            if (DisplayInfo != null && DisplayInfo.Background)
            {
                Color initialColour = DisplayInfo.Background.color;

                if (BackgroundTween != null) TweenExecution.CancelTween(BackgroundTween);
                BackgroundTween = TweenExecution.ApplyTween(0f, 1f, 0.25f, AnimationCurves.EaseType.EaseOutCubic);
                BackgroundTween.OnUpdated = (float value) => DisplayInfo.Background.color = Color.Lerp(initialColour, colour, value);
                BackgroundTween.OnCompleted = (float value) => DisplayInfo.Background.color = colour;
            }
        }

        public void SetWindow(Type origin, Type type, object[] Parameters)
        {
            try
            {
                IWindow tab = GetTab(type);
                tab.CallerWindow = origin;

                WindowManager.SetWindow(tab, Parameters);

                RefreshTime = Time.time;
            }
            catch (Exception exception)
            {
                Logging.Error(string.Concat("SetWindow threw an exception: ", exception.ToString()));
            }
        }

        private IWindow GetTab(Type type)
        {
            if (WindowCache.TryGetValue(type, out IWindow view)) return view;

            IWindow newTab = null;
            try
            {
                newTab = WindowFactory.Create(type);
                WindowCache.Add(type, newTab);

                Logging.Info(string.Concat("Created new tab of type ", nameof(type)));
            }
            catch (Exception exception)
            {
                Logging.Error(string.Concat("GetTab threw an exception: ", exception.ToString()));
            }

            return newTab;
        }

        private void OnEventReceived(EventData photonEvent)
        {
            if (photonEvent.Code == GorillaTagManager.ReportInfectionTagEvent) // The game doesn't use this constant, rather it's hardcoded into the event for some reason
            {
                object[] data = (object[])photonEvent.CustomData;
                Player taggingPlayer = GameMode.ParticipatingPlayers.FirstOrDefault(player => player.UserId == (string)data[0]);

                if (taggingPlayer != null && taggingPlayer.IsLocal)
                {
                    DataManager.AddItem("Tags", DataManager.GetItem("Tags", 0) + 1);
                }
            }
        }
    }
}
