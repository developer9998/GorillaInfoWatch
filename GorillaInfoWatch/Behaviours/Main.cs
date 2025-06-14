﻿using BepInEx.Bootstrap;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Logic;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using InfoWatchScreen = GorillaInfoWatch.Models.InfoWatchScreen;
using PushButton = GorillaInfoWatch.Behaviours.UI.PushButtonComponent;
using Random = UnityEngine.Random;
using SnapSlider = GorillaInfoWatch.Behaviours.UI.SnapSliderComponent;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : Singleton<Main>
    {
        // Pages

        private HomeScreen Home;

        private WarningScreen Warning;

        private InboxScreen Inbox;

        private GameObject container;

        private readonly Dictionary<Type, InfoWatchScreen> registry = [];

        public InfoWatchScreen CurrentScreen;
        private readonly List<InfoWatchScreen> history = [];

        private readonly List<Notification> notifications = [];

        private InfoWatchData data;

        private FigurePredicate[] figures;

        private ItemPredicate[] cosmetics;

        private PlayerPredicate watch;

        // Assets
        public GameObject WatchAsset;
        private GameObject watchObject, menu;

        private InfoWatch localInfoWatch;
        private Panel localPanel;

        // Menu
        private Image menu_background;
        private TMP_Text menu_header;
        private TMP_Text menu_description;
        private Transform menu_line_tform;
        private List<InfoWatchLine> menu_lines;

        // Display
        private TMP_Text menu_page_text;
        private PushButton button_prev_page, button_next_page, button_return_screen, button_reload_screen, buttonOpenInbox;

        // Assets
        public readonly Dictionary<InfoWatchSound, AudioClip> Sounds = [];

        public Dictionary<InfoWatchSymbol, Sprite> Sprites;

        public readonly Dictionary<NetPlayer, InfoWatchSymbol> PlayerSymbol = [];

        public async override void Initialize()
        {
            enabled = false;

            GFriendUtils.InitializeLib(Chainloader.PluginInfos);

            // Screens

            container = new GameObject("ScreenContainer");
            container.transform.SetParent(transform);
            container.SetActive(false);

            var builtinPages = new List<Type>()
            {
                typeof(ScoreboardScreen),
                typeof(PlayerInfoPage),
                typeof(DetailsScreen),
                typeof(FriendScreen),
                typeof(ModListPage),
                typeof(ModInfoPage),
                typeof(CreditScreen)
            };

            builtinPages.ForEach(page => RegisterScreen(page));

            Type baseScreen = typeof(InfoWatchScreen);

            try
            {
                // var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                var assemblies = Chainloader.PluginInfos.Values.Select(info => info.Instance.GetType().Assembly).Distinct();

                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        if (assembly is null) continue;

                        Logging.Info($"Searching assembly {assembly.GetName().Name}");

                        assembly.GetTypes().Where(type => type.IsSubclassOf(baseScreen) && baseScreen.IsAssignableFrom(baseScreen)).ForEach(page => RegisterScreen(page));
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Could not search assembly {assembly.GetName().Name}");
                        Logging.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("Exception thrown when performing initial assembly search");
                Logging.Error(ex);
            }

            container.SetActive(true);

            Home = GetScreen(typeof(HomeScreen)) as HomeScreen;
            Warning = GetScreen(typeof(WarningScreen)) as WarningScreen;
            Inbox = GetScreen(typeof(InboxScreen)) as InboxScreen;

            // Assets

            data = await AssetLoader.LoadAsset<InfoWatchData>("Data");

            figures = Array.ConvertAll(data.Figures, figure => (FigurePredicate)figure);

            cosmetics = Array.ConvertAll(data.Cosmetics, item => (ItemPredicate)item);

            watch = new(InfoWatchSymbol.InfoWatch);

            CheckPlayer(NetworkSystem.Instance.LocalPlayer);

            foreach (string watchSoundName in Enum.GetNames(typeof(InfoWatchSound)))
            {
                AudioClip clip = await AssetLoader.LoadAsset<AudioClip>(watchSoundName);
                if (clip)
                {
                    Sounds.TryAdd(Enum.Parse<InfoWatchSound>(watchSoundName), clip);
                    continue;
                }
                Logging.Warning($"Missing AudioClip asset for sound: {watchSoundName}");
            }

            var sprite_assets = AssetLoader.Bundle.LoadAssetWithSubAssets<Sprite>("Sheet");
            Sprites = Array.FindAll(sprite_assets, sprite => Enum.IsDefined(typeof(InfoWatchSymbol), sprite.name)).ToDictionary(sprite => (InfoWatchSymbol)Enum.Parse(typeof(InfoWatchSymbol), sprite.name), sprite => sprite);

            // Objects

            WatchAsset = await AssetLoader.LoadAsset<GameObject>("Watch25");
            watchObject = Instantiate(WatchAsset);
            watchObject.name = "InfoWatch";

            localInfoWatch = watchObject.AddComponent<InfoWatch>();
            localInfoWatch.Rig = GorillaTagger.Instance.offlineVRRig;

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            float timeOffset = (float)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            localInfoWatch.TimeOffset = timeOffset;
            NetworkHandler.Instance.SetProperty("TimeOffset", timeOffset);

            menu = Instantiate(await AssetLoader.LoadAsset<GameObject>("Menu"));
            menu.name = "InfoMenu";
            menu.gameObject.SetActive(true);

            menu_background = menu.transform.Find("Canvas/Background").GetComponent<Image>();
            menu_header = menu.transform.Find("Canvas/Header/Title").GetComponent<TMP_Text>();
            menu_description = menu.transform.Find("Canvas/Header/Description").GetComponent<TMP_Text>();
            menu_line_tform = menu.transform.Find("Canvas/Lines");
            menu_lines = new(Constants.SectionCapacity);

            foreach (Transform line_tform in menu_line_tform)
            {
                line_tform.gameObject.SetActive(true);
                var line_component = line_tform.gameObject.AddComponent<InfoWatchLine>();
                menu_lines.Add(line_component);
            }

            // Components

            WatchTrigger watchTrigger = watchObject.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();
            watchTrigger.Menu = menu;

            menu_page_text = menu.transform.Find("Canvas/Page Text").GetComponent<TMP_Text>();

            button_next_page = menu.transform.Find("Canvas/Button_Next").AddComponent<PushButton>();
            button_next_page.OnPressed = () =>
            {
                CurrentScreen.Section++;
                RefreshScreen();
            };

            button_prev_page = menu.transform.Find("Canvas/Button_Previous").AddComponent<PushButton>();
            button_prev_page.OnPressed = () =>
            {
                CurrentScreen.Section--;
                RefreshScreen();
            };

            button_return_screen = menu.transform.Find("Canvas/Button_Return").AddComponent<PushButton>();
            button_return_screen.OnPressed = () =>
            {
                SwitchScreen(CurrentScreen.CallerType);
            };

            button_reload_screen = menu.transform.Find("Canvas/Button_Redraw").AddComponent<PushButton>();
            button_reload_screen.OnPressed = () =>
            {
                if (CurrentScreen is InfoWatchScreen screen)
                {
                    screen.OnScreenRefresh();
                    RefreshScreen();
                }
            };

            buttonOpenInbox ??= menu.transform.Find("Canvas/Button_Inbox").AddComponent<PushButton>();
            buttonOpenInbox.OnPressed = delegate ()
            {
                SwitchScreen(Inbox);
            };

            localPanel = menu.AddComponent<Panel>();
            localPanel.Origin = watchObject.transform.Find("Point");

            Events.OnNotificationSent = SendNotification;
            Events.OnNotificationOpened = OpenNotification;

            // Multicasted delegates aka. Events
            RoomSystem.JoinedRoomEvent += OnJoinedRoom;
            NetworkSystem.Instance.OnPlayerJoined += OnPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;
            Events.OnCompleteQuest += OnQuestCompleted;
            CosmeticsController.instance.OnCosmeticsUpdated += delegate ()
            {
                CheckPlayer(NetworkSystem.Instance.LocalPlayer);
            };

            Home.SetEntries([.. registry.Values]);
            SwitchScreen(Home);

            enabled = true;
        }

        public void SwitchScreen(InfoWatchScreen newScreen)
        {
            if (CurrentScreen is InfoWatchScreen lastScreen)
            {
                CurrentScreen.RequestScreenSwitch -= SwitchScreen;
                CurrentScreen.RequestSetLines -= RefreshScreen;

                CurrentScreen.enabled = false;

                CurrentScreen.OnScreenClose();

                if (history.Count == 0 || history.Last() != newScreen) history.Add(CurrentScreen);
            }

            CurrentScreen = newScreen;

            if (newScreen == Home) history.Clear();
            else if (history.Count > 0 && history.Last() == newScreen) history.RemoveAt(history.Count - 1);

            newScreen.enabled = true;
            newScreen.CallerType = history.Count > 0 ? history.Last().GetType() : null;

            newScreen.RequestScreenSwitch += SwitchScreen;
            newScreen.RequestSetLines += RefreshScreen;

            newScreen.OnScreenOpen();

            RefreshScreen();
        }

        public void SwitchScreen(Type type)
        {
            if (type is null)
                SwitchScreen(Home);
            else if (GetScreen(type) is InfoWatchScreen screen)
                SwitchScreen(screen);
        }

        public void RefreshScreen(bool includeWidgets = true)
        {
            buttonOpenInbox.gameObject.SetActive(CurrentScreen == Home);
            button_return_screen.gameObject.SetActive(CurrentScreen != Home);

            var content = CurrentScreen.GetContent() ?? new LineBuilder();

            try
            {
                int sectionCount = content.SectionCount();
                CurrentScreen.Section = sectionCount != 0 ? CurrentScreen.Section.Wrap(0, sectionCount) : 0;

                bool multiSection = sectionCount > 1;
                button_next_page.gameObject.SetActive(multiSection);
                button_prev_page.gameObject.SetActive(multiSection);
                menu_page_text.text = multiSection ? $"{CurrentScreen.Section + 1}/{sectionCount}" : string.Empty;

                string sectionTitle = content.SectionTitle(CurrentScreen.Section);
                menu_header.text = $"{CurrentScreen.Title}{(string.IsNullOrEmpty(sectionTitle) ? "" : $" - {sectionTitle}")}";

                if (string.IsNullOrEmpty(CurrentScreen.Description) && menu_description.gameObject.activeSelf)
                {
                    menu_description.gameObject.SetActive(false);
                }
                else if (!string.IsNullOrEmpty(CurrentScreen.Description))
                {
                    menu_description.gameObject.SetActive(true);
                    menu_description.text = CurrentScreen.Description;
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Menu could not adjust for ({CurrentScreen.GetType().Name})");
                Logging.Error(ex);
            }

            try
            {
                IEnumerable<ScreenLine> lines = content.SectionLines(CurrentScreen.Section);

                for (int i = 0; i < Constants.SectionCapacity; i++)
                {
                    InfoWatchLine menu_line = menu_lines[i];

                    if (lines.ElementAtOrDefault(i) is ScreenLine line)
                    {
                        bool wasLineActive = menu_line.gameObject.activeSelf;
                        menu_line.gameObject.SetActive(true);
                        menu_line.Build(line, !wasLineActive || includeWidgets);

                        continue;
                    }

                    menu_line.gameObject.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Screen contents could not be displayed ({CurrentScreen.GetType().Name})");
                Logging.Error(ex);
            }
        }

        public bool RegisterScreen(Type type)
        {
            Logging.Info($"RegisterScreen: {type.Name} of {type.Namespace}");

            if (registry.ContainsKey(type))
            {
                Logging.Warning("Registry contains key");
                return false;
            }

            Type baseScreen = typeof(InfoWatchScreen);

            if (!type.IsSubclassOf(baseScreen))
            {
                Logging.Warning("Type is not subclass of screen");
                return false;
            }

            if (!baseScreen.IsAssignableFrom(type))
            {
                Logging.Warning("Type is not assignable from screen");
                return false;
            }

            Component component = container.AddComponent(type);

            if (component is InfoWatchScreen screen)
            {
                if (registry.ContainsValue(screen))
                {
                    Logging.Warning("Registry contains value (component of type)");
                    Destroy(component);
                    return false;
                }

                screen.enabled = false;

                registry[type] = screen;

                Logging.Info("Register success");
                return true;
            }

            Logging.Warning("Component of type is not screen (this shouldn't happen)");
            Destroy(component);
            return false;
        }

        public InfoWatchScreen GetScreen(Type type, bool registerFallback = true)
        {
            return (registry.ContainsKey(type) || (registerFallback && RegisterScreen(type))) ? registry[type] : null;
        }

        public void PlayErrorSound()
        {
            if (Enum.Parse<InfoWatchSound>(string.Concat("error", Random.Range(1, 6))) is InfoWatchSound result && Sounds.TryGetValue(result, out AudioClip audioClip))
                localInfoWatch.AudioDevice.PlayOneShot(audioClip);
        }

        public void SendNotification(Notification notification)
        {
            if (notification is null || notification.Opened || notifications.Contains(notification))
                return;

            for (int i = 0; i < notifications.Count; i++)
            {
                if (notifications[i].Content == notification.Content)
                {
                    notifications.RemoveAt(i);
                    i--;
                }
            }

            notifications.Add(notification);

            if (Sounds.TryGetValue(notification.Sound, out AudioClip audio))
                localInfoWatch.AudioDevice.PlayOneShot(audio);

            GorillaTagger.Instance.StartVibration(localInfoWatch.InLeftHand, 0.04f, 0.2f);

            var stateMachine = localInfoWatch.stateMachine;
            Menu_StateBase currentState = stateMachine.CurrentState is Menu_SubState subState ? subState.previousState : stateMachine.CurrentState;
            stateMachine.SwitchState(new Menu_Notification(localInfoWatch, currentState, notification));

            RefreshInbox();
        }

        public void OpenNotification(Notification notification, bool digest)
        {
            if (notification is null || notification.Opened || !notifications.Contains(notification))
                return;

            notification.Opened = true;

            if (digest && notification.Screen is not null && GetScreen(notification.Screen.ScreenType) is InfoWatchScreen screen)
            {
                if (notification.Screen.Task is Task task)
                {
                    TaskAwaiter awaiter = task.GetAwaiter();
                    awaiter.OnCompleted(delegate ()
                    {
                        SwitchScreen(screen);
                    });
                    awaiter.GetResult();
                }
                else
                {
                    SwitchScreen(screen);
                }
            }

            RefreshInbox();
        }

        public void RefreshInbox()
        {
            Inbox.Inbox = [.. notifications.Where(notif => !notif.Opened).OrderBy(notif => notif.Created)];

            if (CurrentScreen == Inbox)
                RefreshScreen();
        }

        public void OnJoinedRoom()
        {
            foreach(NetPlayer player in NetworkSystem.Instance.AllNetPlayers)
            {
                CheckPlayer(player);
            }
        }

        public void OnPlayerJoined(NetPlayer player)
        {
            CheckPlayer(player);

            if (GFriendUtils.IsFriend(player.UserId))
            {
                Events.SendNotification
                (
                    new
                    (
                        "Your friend has joined",
                        string.Format
                        (
                            "<color=#{0}>{1}</color>",
                            ColorUtility.ToHtmlStringRGB(GFriendUtils.FriendColour),
                            player.NickName.SanitizeName()
                        ),
                        5, InfoWatchSound.notificationPositive,
                        new Notification.ExternalScreen
                        (
                            typeof(PlayerInfoPage),
                            $"Inspect {player.NickName.SanitizeName()}",
                            delegate ()
                            {
                                if (VRRigCache.Instance.TryGetVrrig(player, out RigContainer playerRig))
                                    PlayerInfoPage.Container = playerRig;
                            }
                        )
                    )
                );
            }
        }

        public void OnPlayerLeft(NetPlayer player)
        {
            if (GFriendUtils.IsFriend(player.UserId))
            {
                Events.SendNotification
                (
                    new
                    (
                        "Your friend has left",
                        string.Format
                        (
                            "<color=#{0}>{1}</color>",
                            ColorUtility.ToHtmlStringRGB(GFriendUtils.FriendColour),
                            player.NickName.SanitizeName()
                        ),
                        5, InfoWatchSound.notificationNegative
                    )
                );
            }
        }

        public void CheckPlayer(NetPlayer player)
        {
            if (player is null || player.IsNull)
                return;

            PlayerPredicate predicate = null;

            if (Array.Find(figures, figure => figure.IsValid(player)) is FigurePredicate figure)
                predicate = figure;
            else if (Array.Find(cosmetics, cosmetic => cosmetic.IsValid(player)) is ItemPredicate cosmetic)
                predicate = cosmetic;
            else if (player.IsLocal || VRRigCache.Instance.TryGetVrrig(player, out RigContainer playerRig) && playerRig.TryGetComponent(out NetworkedPlayer component) && component.HasInfoWatch)
                predicate = watch;

            if (predicate is not null)
            {
                if (PlayerSymbol.ContainsKey(player))
                    PlayerSymbol[player] = predicate.Symbol;
                else
                    PlayerSymbol.Add(player, predicate.Symbol);
            }
        }

        public void OnQuestCompleted(RotatingQuestsManager.RotatingQuest quest)
        {
            Events.SendNotification
            (
                new
                (
                    "You completed a quest",
                    quest.GetTextDescription(),
                    5, InfoWatchSound.notificationNeutral
                )
            );
        }

        public void PressButton(PushButton button, bool isLeftHand)
        {
            if (button)
            {
                AudioSource handSource = isLeftHand
                    ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                    : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : Sounds[InfoWatchSound.widgetButton];
                handSource.PlayOneShot(clip, 0.2f);
            }
        }

        public void PressSlider(SnapSlider slider, bool isLeftHand)
        {
            if (slider)
            {
                AudioSource handSource = isLeftHand
                    ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                    : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

                AudioClip clip = Sounds[InfoWatchSound.widgetButton];
                handSource.PlayOneShot(clip, 0.2f);
            }
        }

        public void PressSwitch(SwitchComponent button, bool isLeftHand)
        {
            if (button)
            {
                AudioSource handSource = isLeftHand
                    ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                    : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : Sounds[InfoWatchSound.widgetSwitch];
                handSource.PlayOneShot(clip, 0.2f);
            }
        }
    }
}
