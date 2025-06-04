using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = GorillaInfoWatch.Behaviours.Widgets.Button;
using SnapSlider = GorillaInfoWatch.Behaviours.Widgets.SnapSlider;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : Singleton<Main>
    {
        // Pages

        private HomeScreen HomePage;

        private WarningScreen WarnPage;

        private GameObject screen_container;

        public List<WatchScreen> ScreenRegistry = [];

        public WatchScreen CurrentScreen;
        private readonly List<WatchScreen> screen_history = [];

        // Assets
        public GameObject WatchAsset;
        private GameObject watchObject, menu;

        private Watch localPlayerWatch;
        private Panel localPlayerPanel;

        // Menu
        private Image menu_background;
        private TMP_Text menu_header;
        private TMP_Text menu_description;
        private Transform menu_line_tform;
        private List<MenuLine> menu_lines;

        // Display
        private TMP_Text menu_page_text;
        private Button button_prev_page, button_next_page, button_return_screen, button_reload_screen;

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

            screen_container = new GameObject("Screen Container");
            screen_container.transform.SetParent(transform, false);
            screen_container.SetActive(false);

            var builtinPages = new List<Type>()
            {
                typeof(ScoreboardScreen),
                typeof(DetailsScreen),
                typeof(FriendScreen),
                typeof(ModStatusPage),
                typeof(CreditScreen)
            };

            builtinPages.ForEach(RegisterScreen);

            try
            {
                //var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var assemblies = Chainloader.PluginInfos.Values.Select(info => info.Instance.GetType().Assembly).Distinct();

                foreach(Assembly assembly in assemblies)
                {
                    try
                    {
                        if (assembly is null) continue;
                        Logging.Info(assembly.GetName().Name);
                        if (assembly.GetCustomAttribute<WatchCompatibleModAttribute>() is not null)
                        {
                            Logging.Info($"Searching assembly {assembly.GetName().Name}");

                            var types = assembly.GetTypes();
                            types.Where(page => page.GetCustomAttribute<WatchCustomPageAttribute>() != null).ForEach(RegisterScreen);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("Exception thrown when performing initial assembly check");
                Logging.Error(ex);
            }

            screen_container.SetActive(true);

            HomePage = gameObject.AddComponent<HomeScreen>();
            WarnPage = gameObject.AddComponent<WarningScreen>();

            // Assets

            foreach(string watchSoundName in Enum.GetNames(typeof(EWatchSound)))
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

            localPlayerWatch = watchObject.AddComponent<Watch>();
            localPlayerWatch.Rig = GorillaTagger.Instance.offlineVRRig;

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            string timeZone = (timeZoneInfo.SupportsDaylightSavingTime && timeZoneInfo.IsDaylightSavingTime(DateTime.Now)) ? timeZoneInfo.DaylightName : timeZoneInfo.StandardName;
            localPlayerWatch.TimeZone = timeZone;
            NetworkHandler.Instance.SetProperty("TimeZone", timeZone);

            float timeOffset = (float)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            localPlayerWatch.TimeOffset = timeOffset;
            NetworkHandler.Instance.SetProperty("TimeOffset", timeOffset);

            menu = Instantiate(await AssetLoader.LoadAsset<GameObject>("Menu"));
            menu.name = "InfoMenu";

            localPlayerPanel = menu.AddComponent<Panel>();
            localPlayerPanel.Origin = watchObject.transform.Find("Point");

            menu_background = menu.transform.Find("Canvas/Background").GetComponent<Image>();
            menu_header = menu.transform.Find("Canvas/Header/Title").GetComponent<TMP_Text>();
            menu_description = menu.transform.Find("Canvas/Header/Description").GetComponent<TMP_Text>();
            menu_line_tform = menu.transform.Find("Canvas/Lines");
            menu_lines = new(Constants.LinesPerPage);

            foreach (Transform line_tform in menu_line_tform)
            {
                line_tform.gameObject.SetActive(false);
                var line_component = line_tform.gameObject.AddComponent<MenuLine>();
                menu_lines.Add(line_component);
            }

            // Components

            WatchTrigger watchTrigger = watchObject.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();
            watchTrigger.Menu = menu;

            menu_page_text = menu.transform.Find("Canvas/Page Text").GetComponent<TMP_Text>();

            button_next_page = menu.transform.Find("Canvas/Button_Next").AddComponent<Button>();
            button_next_page.OnPressed = () =>
            {
                CurrentScreen.PageNumber++;
                DisplayScreen();
            };

            button_prev_page = menu.transform.Find("Canvas/Button_Previous").AddComponent<Button>();
            button_prev_page.OnPressed = () =>
            {
                CurrentScreen.PageNumber--;
                DisplayScreen();
            };

            button_return_screen = menu.transform.Find("Canvas/Button_Return").AddComponent<Button>();
            button_return_screen.OnPressed = () =>
            {
                if (screen_history.Count > 0)
                {
                    SwitchScreen(screen_history[^1]);
                }
            };

            button_reload_screen = menu.transform.Find("Canvas/Button_Redraw").AddComponent<Button>();
            button_reload_screen.OnPressed = () =>
            {
                if (CurrentScreen is WatchScreen screen)
                {
                    DisplayScreen();
                }
            };

            NetworkSystem.Instance.OnPlayerJoined += OnPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;
            PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;

            HomePage.SetEntries(ScreenRegistry);
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
            if (TryGetComponent(type, out Component component) && component is WatchScreen screen)
            {
                SwitchScreen(screen);
                return;
            }

            RegisterScreen(type);
            SwitchScreen(ScreenRegistry[^1]); // latest screen
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
            catch(Exception ex)
            {
                Logging.Error($"Exception thrown when displaying screen {CurrentScreen.GetType().Name}: {ex}");
            }

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

        public void RegisterScreen(Type type)
        {
            try
            {
                var component = screen_container.AddComponent(type);

                if (component is WatchScreen page)
                {
                    Logging.Info($"Added Type {type.Name}");

                    page.enabled = false;
                    RegisterScreen(page);

                    return;
                }

                Logging.Warning($"Type {type.Name} is not WatchScreen");

                Destroy(component);
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Exception thrown when casting Page from Type {type.FullName}");
                Logging.Error(ex);
            }
        }

        public void RegisterScreen(WatchScreen page)
        {
            if (ScreenRegistry.Contains(page))
            {
                Logging.Warning($"Page {page.GetType().Name} is already included in registry");
                return;
            }

            ScreenRegistry.Add(page);
        }

        public void OnPlayerJoined(NetPlayer player)
        {
            try
            {
                if (FriendLib.IsFriend(player.UserId))
                {
                    localPlayerWatch.DisplayMessage(string.Format("<size=60%>A FRIEND HAS JOINED:</size><br><b><color=#{0}>{1}</color></b>", ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour), player.NickName.SanitizeName()), 4f, EWatchSound.NotifPos);
                }
            }
            catch(Exception ex)
            {
                Logging.Fatal("Main.OnPlayerJoined");
                Logging.Error(ex);
            }
        }

        public void OnPlayerLeft(NetPlayer player)
        {
            try
            {
                if (FriendLib.IsFriend(player.UserId))
                {
                    localPlayerWatch.DisplayMessage(string.Format("<size=60%>A FRIEND HAS LEFT:</size><br><b><color=#{0}>{1}</color></b>", ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour), player.NickName.SanitizeName()), 4f, EWatchSound.NotifNeg);
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("Main.OnPlayerLeft");
                Logging.Error(ex);
            }
        }

        public void PressButton(Button button, bool isLeftHand)
        {
            if (button)
            {
                AudioSource handSource = isLeftHand
                    ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                    : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : Sounds[EWatchSound.Tap];
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

                AudioClip clip = Sounds[EWatchSound.Tap];
                handSource.PlayOneShot(clip, 0.2f);
            }
        }

        private void OnEventReceived(EventData photonEvent)
        {
            if (photonEvent.Code == GorillaTagManager.ReportInfectionTagEvent) // The game doesn't use this constant, rather it's hardcoded into the event for some reason
            {
                object[] data = (object[])photonEvent.CustomData;

                NetPlayer taggingNetPlayer = GameMode.ParticipatingPlayers.FirstOrDefault(player => player.UserId == (string)data[0]);

                if (taggingNetPlayer != null && taggingNetPlayer.IsLocal)
                {
                    Singleton<DataHandler>.Instance.AddItem("Tags", Singleton<DataHandler>.Instance.GetItem("Tags", 0) + 1);
                }
            }
        }
    }
}
