using BepInEx.Bootstrap;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Behaviours.Widgets;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PushButtonComponent = GorillaInfoWatch.Behaviours.Widgets.PushButtonComponent;
using SnapSliderComponent = GorillaInfoWatch.Behaviours.Widgets.SnapSliderComponent;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : Singleton<Main>
    {
        // Pages

        private HomeScreen HomePage;

        private WarningScreen WarnPage;

        private GameObject container;

        public Dictionary<Type, WatchScreen> ScreenRegistry = [];

        public WatchScreen CurrentScreen;
        private readonly List<WatchScreen> screen_history = [];

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
        private PushButtonComponent button_prev_page, button_next_page, button_return_screen, button_reload_screen;

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
                (netPlayer) => netPlayer != null && RigUtils.TryGetVRRig(netPlayer, out var playerRig) && RigUtils.HasItem(playerRig, "LBAAK.", false),
                EDefaultSymbol.StickCosmetic
            },
            {
                (netPlayer) => netPlayer != null && RigUtils.TryGetVRRig(netPlayer, out var playerRig) && RigUtils.HasItem(playerRig, "LBADE.", false),
                EDefaultSymbol.FingerPainterBadge
            },
            {
                (netPlayer) => netPlayer != null && RigUtils.TryGetVRRig(netPlayer, out var playerRig) && RigUtils.HasItem(playerRig, "LBAGS.", false),
                EDefaultSymbol.IllustratorBadge
            }
        };

        public async override void Initialize()
        {
            enabled = false;

            FriendLib.InitializeLib(Chainloader.PluginInfos);

            // Screens

            container = new GameObject("ScreenContainer");
            container.transform.SetParent(transform);
            container.SetActive(false);

            var builtinPages = new List<Type>()
            {
                typeof(ScoreboardScreen),
                typeof(DetailsScreen),
                typeof(FriendScreen),
                typeof(ModListPage),
                typeof(CreditScreen)
            };

            builtinPages.ForEach(page => RegisterScreen(page));

            try
            {
                //var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var assemblies = Chainloader.PluginInfos.Values.Select(info => info.Instance.GetType().Assembly).Distinct();

                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        if (assembly is null) continue;

                        Logging.Info(assembly.GetName().Name);

                        if (assembly.GetCustomAttribute<WatchCompatibleModAttribute>() is not null)
                        {
                            Logging.Info($"Searching assembly {assembly.GetName().Name}");

                            assembly
                                .GetTypes()
                                .Where(page => page.GetCustomAttribute<WatchCustomPageAttribute>() is not null)
                                .ForEach(page => RegisterScreen(page));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Could not check assembly {assembly.GetName().Name}");
                        Logging.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("Exception thrown when performing initial assembly check");
                Logging.Error(ex);
            }

            container.SetActive(true);

            HomePage = GetScreen(typeof(HomeScreen)) as HomeScreen;
            WarnPage = GetScreen(typeof(WarningScreen)) as WarningScreen;

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

            button_next_page = menu.transform.Find("Canvas/Button_Next").AddComponent<PushButtonComponent>();
            button_next_page.OnPressed = () =>
            {
                CurrentScreen.PageNumber++;
                DisplayScreen();
            };

            button_prev_page = menu.transform.Find("Canvas/Button_Previous").AddComponent<PushButtonComponent>();
            button_prev_page.OnPressed = () =>
            {
                CurrentScreen.PageNumber--;
                DisplayScreen();
            };

            button_return_screen = menu.transform.Find("Canvas/Button_Return").AddComponent<PushButtonComponent>();
            button_return_screen.OnPressed = () =>
            {
                if (screen_history.Count > 0)
                {
                    SwitchScreen(screen_history[^1]);
                }
            };

            button_reload_screen = menu.transform.Find("Canvas/Button_Redraw").AddComponent<PushButtonComponent>();
            button_reload_screen.OnPressed = () =>
            {
                if (CurrentScreen is WatchScreen screen)
                {
                    screen.OnScreenRefresh();
                    DisplayScreen();
                }
            };

            NetworkSystem.Instance.OnPlayerJoined += OnPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;
            Events.OnCompleteQuest += OnQuestCompleted;

            HomePage.SetEntries([.. ScreenRegistry.Values]);
            SwitchScreen(HomePage);

            enabled = true;
        }

        public void SwitchScreen(WatchScreen newScreen)
        {
            if (CurrentScreen is WatchScreen lastScreen)
            {
                CurrentScreen.RequestScreenSwitch -= SwitchScreen;
                CurrentScreen.RequestSetLines -= DisplayScreen;
                CurrentScreen.enabled = false;
                CurrentScreen.OnScreenClose();

                if (screen_history.Count == 0 || screen_history[^1] != newScreen) screen_history.Add(CurrentScreen);
            }

            CurrentScreen = newScreen;

            if (screen_history.Count > 0 && screen_history[^1] == newScreen) screen_history.RemoveAt(screen_history.Count - 1);

            newScreen.enabled = true;
            newScreen.RequestScreenSwitch += SwitchScreen;
            newScreen.RequestSetLines += DisplayScreen;
            newScreen.OnScreenOpen();

            DisplayScreen();
        }

        public void SwitchScreen(Type type)
        {
            if (GetScreen(type) is WatchScreen screen)
                SwitchScreen(screen);
        }

        public void DisplayScreen(bool includeWidgets = true)
        {
            button_return_screen.gameObject.SetActive(screen_history.Count > 0);

            var content = CurrentScreen.GetContent();
            if (content == null) return;

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

            Type baseScreen = typeof(WatchScreen);

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

            if (component is WatchScreen page)
            {
                if (ScreenRegistry.ContainsValue(page))
                {
                    Logging.Warning("Registry contains value (component of type)");
                    Destroy(component);
                    return false;
                }

                page.enabled = false;

                ScreenRegistry[type] = page;

                Logging.Info("Register success");
                return true;
            }

            Logging.Warning("Component of type is not screen (this shouldn't happen)");
            Destroy(component);
            return false;
        }

        public WatchScreen GetScreen(Type type)
        {
            return (ScreenRegistry.ContainsKey(type) || RegisterScreen(type)) ? ScreenRegistry[type] : null;
        }

        public void PlayErrorSound()
        {
            if (Enum.Parse<EWatchSound>(string.Concat("error", UnityEngine.Random.Range(1, 6))) is EWatchSound result && Sounds.TryGetValue(result, out AudioClip audioClip))
                localInfoWatch.AudioDevice.PlayOneShot(audioClip);
        }

        public void OnPlayerJoined(NetPlayer player)
        {
            if (FriendLib.IsFriend(player.UserId))
            {
                localInfoWatch.DisplayMessage
                (
                    string.Format
                    (
                        "<size=6>Your friend has joined:</size><br><b><color=#{0}>{1}</color></b>",
                        ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour),
                        player.NickName.SanitizeName()
                    ),
                    5,
                    EWatchSound.notificationPositive,
                    GetScreen(typeof(PlayerInfoPage)),
                    delegate ()
                    {
                        if (RigUtils.TryGetVRRig(player, out RigContainer playerRig))
                            PlayerInfoPage.Container = playerRig;
                    }
                );
            }
        }

        public void OnPlayerLeft(NetPlayer player)
        {
            if (FriendLib.IsFriend(player.UserId))
            {
                localInfoWatch.DisplayMessage
                (
                    string.Format
                    (
                        "<size=6>Your friend has left:</size><br><b><color=#{0}>{1}</color></b>",
                        ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour),
                        player.NickName.SanitizeName()
                    ),
                    5,
                    EWatchSound.notificationNegative
                );
            }
        }

        public void OnQuestCompleted(RotatingQuestsManager.RotatingQuest quest)
        {
            localInfoWatch.DisplayMessage
            (
                string.Format
                (
                    "<size=6>You completed a quest:</size><br><b>{0}</b>",
                    quest.GetTextDescription()
                ),
                3,
                EWatchSound.notificationNeutral
            );
        }

        public void PressButton(PushButtonComponent button, bool isLeftHand)
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

        public void PressSlider(SnapSliderComponent slider, bool isLeftHand)
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
