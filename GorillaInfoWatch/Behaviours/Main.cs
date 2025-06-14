using BepInEx.Bootstrap;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Behaviours.Widgets;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PushButton = GorillaInfoWatch.Behaviours.Widgets.PushButtonComponent;
using Screen = GorillaInfoWatch.Models.Screen;
using SnapSlider = GorillaInfoWatch.Behaviours.Widgets.SnapSliderComponent;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : Singleton<Main>
    {
        // Pages

        private HomeScreen Home;

        private WarningScreen Warning;

        private InboxScreen Inbox;

        private GameObject container;

        public Dictionary<Type, Models.Screen> ScreenRegistry = [];

        public Screen CurrentScreen;
        private readonly List<Screen> history = [];

        private readonly List<Notification> notifications = [];

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
        private List<Line> menu_lines;

        // Display
        private TMP_Text menu_page_text;
        private PushButton button_prev_page, button_next_page, button_return_screen, button_reload_screen, buttonOpenInbox;

        // Assets
        public readonly Dictionary<EWatchSound, AudioClip> Sounds = [];

        public Dictionary<EDefaultSymbol, Sprite> Sprites;

        public Dictionary<Predicate<NetPlayer>, EDefaultSymbol> SpecialSprites = new()
        {
            {
                (netPlayer) => netPlayer != null && (netPlayer.UserId == "E354E818871BD1D8" || netPlayer.UserId == "91902A4E2624708F"),
                EDefaultSymbol.DevSprite
            },
            {
                (netPlayer) => netPlayer != null && netPlayer.UserId == "FBE3EE50747CB892",
                EDefaultSymbol.KaylieSprite
            },
            {
                (netPlayer) => netPlayer != null && netPlayer.UserId == "AA30186DA3713730" && UnityEngine.Random.value <= 0.01f,
                EDefaultSymbol.BloodJmanSprite
            },
            {
                (netPlayer) => netPlayer != null && netPlayer.UserId == "AA30186DA3713730",
                EDefaultSymbol.StaircaseSprite
            },
            {
                (netPlayer) => netPlayer != null && netPlayer.UserId == "149657534CA679F",
                EDefaultSymbol.AstridSprite
            },
            {
                (netPlayer) => netPlayer != null && netPlayer.UserId == "95ACF372B189C4EB",
                EDefaultSymbol.DeactivatedSprite
            },
            {
                (netPlayer) => netPlayer != null && (netPlayer.UserId == "412602E3586FEF3A" || netPlayer.UserId == "383A50F9EB1ACD5A"),
                EDefaultSymbol.CyanSprite
            },
            {
                (netPlayer) => netPlayer != null && netPlayer.UserId == "82CDEE6EC76948D",
                EDefaultSymbol.H4rnsSprite
            },
            {
                (netPlayer) => netPlayer != null && VRRigCache.Instance.TryGetVrrig(netPlayer, out var playerRig) && RigUtilities.HasItem(playerRig, "LBAAK.", false),
                EDefaultSymbol.StickCosmetic
            },
            {
                (netPlayer) => netPlayer != null && VRRigCache.Instance.TryGetVrrig(netPlayer, out var playerRig) && RigUtilities.HasItem(playerRig, "LBADE.", false),
                EDefaultSymbol.FingerPainterBadge
            },
            {
                (netPlayer) => netPlayer != null && VRRigCache.Instance.TryGetVrrig(netPlayer, out var playerRig) && RigUtilities.HasItem(playerRig, "LBAGS.", false),
                EDefaultSymbol.IllustratorBadge
            }
        };

        public async override void Initialize()
        {
            enabled = false;

            FriendUtilities.InitializeLib(Chainloader.PluginInfos);

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

            Type baseScreen = typeof(Screen);

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

            foreach (string watchSoundName in Enum.GetNames(typeof(EWatchSound)))
            {
                AudioClip clip = await AssetLoader.LoadAsset<AudioClip>(watchSoundName);
                if (clip)
                {
                    Sounds.TryAdd(Enum.Parse<EWatchSound>(watchSoundName), clip);
                    continue;
                }
                Logging.Warning($"Missing AudioClip asset for sound: {watchSoundName}");
            }

            var sprite_assets = AssetLoader.Bundle.LoadAssetWithSubAssets<Sprite>("Sheet");
            Sprites = Array.FindAll(sprite_assets, sprite => Enum.IsDefined(typeof(EDefaultSymbol), sprite.name)).ToDictionary(sprite => (EDefaultSymbol)Enum.Parse(typeof(EDefaultSymbol), sprite.name), sprite => sprite);

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
            menu_lines = new(Constants.LinesPerPage);

            foreach (Transform line_tform in menu_line_tform)
            {
                line_tform.gameObject.SetActive(true);
                var line_component = line_tform.gameObject.AddComponent<Line>();
                menu_lines.Add(line_component);
            }

            localPanel = menu.AddComponent<Panel>();
            localPanel.Origin = watchObject.transform.Find("Point");

            // Components

            WatchTrigger watchTrigger = watchObject.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();
            watchTrigger.Menu = menu;

            menu_page_text = menu.transform.Find("Canvas/Page Text").GetComponent<TMP_Text>();

            button_next_page = menu.transform.Find("Canvas/Button_Next").AddComponent<PushButton>();
            button_next_page.OnPressed = () =>
            {
                CurrentScreen.PageNumber++;
                RefreshScreen();
            };

            button_prev_page = menu.transform.Find("Canvas/Button_Previous").AddComponent<PushButton>();
            button_prev_page.OnPressed = () =>
            {
                CurrentScreen.PageNumber--;
                RefreshScreen();
            };

            button_return_screen = menu.transform.Find("Canvas/Button_Return").AddComponent<PushButton>();
            button_return_screen.OnPressed = () =>
            {
                if (CurrentScreen is Screen screen && screen.CallerType is Type callerType)
                {
                    SwitchScreen(callerType);
                }
                else
                {
                    SwitchScreen(Home);
                }
            };

            button_reload_screen = menu.transform.Find("Canvas/Button_Redraw").AddComponent<PushButton>();
            button_reload_screen.OnPressed = () =>
            {
                if (CurrentScreen is Screen screen)
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

            Events.OnNotificationSent = SendNotification;
            Events.OnNotificationOpened = OpenNotification;

            // Multicasted delegates aka. Events
            NetworkSystem.Instance.OnPlayerJoined += OnPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;
            Events.OnCompleteQuest += OnQuestCompleted;

            Home.SetEntries([.. ScreenRegistry.Values]);
            SwitchScreen(Home);

            enabled = true;
        }

        public void SwitchScreen(Screen newScreen)
        {
            if (CurrentScreen is Screen lastScreen)
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
            if (GetScreen(type) is Screen screen)
                SwitchScreen(screen);
        }

        public void RefreshScreen(bool includeWidgets = true)
        {
            buttonOpenInbox.gameObject.SetActive(CurrentScreen == Home);
            button_return_screen.gameObject.SetActive(CurrentScreen != Home);

            var content = CurrentScreen.GetContent();
            if (content is null) return;

            try
            {
                int page_count = content.GetPageCount();
                CurrentScreen.PageNumber = page_count != 0 ? CurrentScreen.PageNumber.Wrap(0, page_count) : 0;

                button_next_page.gameObject.SetActive(page_count > 1);
                button_prev_page.gameObject.SetActive(page_count > 1);
                menu_page_text.text = (button_next_page.gameObject.activeSelf || button_prev_page.gameObject.activeSelf) ? $"{CurrentScreen.PageNumber + 1}/{page_count}" : "";

                string page_title = content.GetPageTitle(CurrentScreen.PageNumber);
                menu_header.text = $"{CurrentScreen.Title}{(string.IsNullOrEmpty(page_title) ? "" : $" - {page_title}")}";
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
                var lines = content.GetPageLines(CurrentScreen.PageNumber);

                for (int i = 0; i < Constants.LinesPerPage; i++)
                {
                    var menu_line = menu_lines[i];

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

            if (ScreenRegistry.ContainsKey(type))
            {
                Logging.Warning("Registry contains key");
                return false;
            }

            Type baseScreen = typeof(Screen);

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

            if (component is Screen screen)
            {
                if (ScreenRegistry.ContainsValue(screen))
                {
                    Logging.Warning("Registry contains value (component of type)");
                    Destroy(component);
                    return false;
                }

                screen.enabled = false;

                ScreenRegistry[type] = screen;

                Logging.Info("Register success");
                return true;
            }

            Logging.Warning("Component of type is not screen (this shouldn't happen)");
            Destroy(component);
            return false;
        }

        public Screen GetScreen(Type type, bool registerFallback = true)
        {
            return (ScreenRegistry.ContainsKey(type) || (registerFallback && RegisterScreen(type))) ? ScreenRegistry[type] : null;
        }

        public void PlayErrorSound()
        {
            if (Enum.Parse<EWatchSound>(string.Concat("error", UnityEngine.Random.Range(1, 6))) is EWatchSound result && Sounds.TryGetValue(result, out AudioClip audioClip))
                localInfoWatch.AudioDevice.PlayOneShot(audioClip);
        }

        public void SendNotification(Notification notification)
        {
            if (notification is null || notification.IsOpened || notifications.Contains(notification))
                return;

            notifications.Add(notification);

            if (Sounds.TryGetValue(notification.Sound, out AudioClip audio))
                localInfoWatch.AudioDevice.PlayOneShot(audio);

            GorillaTagger.Instance.StartVibration(localInfoWatch.InLeftHand, 0.04f, 0.2f);

            var stateMachine = localInfoWatch.stateMachine;
            Menu_StateBase currentState = stateMachine.CurrentState is Menu_SubState subState ? subState.previousState : stateMachine.CurrentState;
            stateMachine.SwitchState(new Menu_Notification(localInfoWatch, currentState, notification));

            RefreshInbox();
        }

        public void OpenNotification(Notification notification)
        {
            if (notification is null || notification.IsOpened || !notifications.Contains(notification))
                return;

            notification.IsOpened = true;

            if (notification.Screen is not null && GetScreen(notification.Screen.ScreenType) is Screen screen)
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
            Inbox.Inbox = [.. notifications.Where(notif => !notif.IsOpened).OrderBy(notif => notif.Created)];

            if (CurrentScreen == Inbox)
                RefreshScreen();
        }

        public void OnPlayerJoined(NetPlayer player)
        {
            if (FriendUtilities.IsFriend(player.UserId))
            {
                Events.SendNotification
                (
                    new
                    (
                        "Your friend has joined",
                        string.Format
                        (
                            "<color=#{0}>{1}</color>",
                            ColorUtility.ToHtmlStringRGB(FriendUtilities.FriendColour),
                            player.NickName.SanitizeName()
                        ),
                        5, EWatchSound.notificationPositive,
                        new Notification.ExternalScreen
                        (
                            typeof(PlayerInfoPage),
                            $"Inspect {player.NickName.SanitizeName()}",
                            delegate()
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
            if (FriendUtilities.IsFriend(player.UserId))
            {
                Events.SendNotification
                (
                    new
                    (
                        "Your friend has left",
                        string.Format
                        (
                            "<color=#{0}>{1}</color>",
                            ColorUtility.ToHtmlStringRGB(FriendUtilities.FriendColour),
                            player.NickName.SanitizeName()
                        ),
                        5, EWatchSound.notificationNegative
                    )
                );
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
                    5, EWatchSound.notificationNeutral
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

                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : Sounds[EWatchSound.widgetButton];
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

                AudioClip clip = Sounds[EWatchSound.widgetButton];
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

                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : Sounds[EWatchSound.widgetSwitch];
                handSource.PlayOneShot(clip, 0.2f);
            }
        }
    }
}
