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
        private GameObject watch, menu;

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
        public Dictionary<string, AudioClip> Sounds;

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
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                assemblies.Where(assembly => assembly != null && assembly.GetCustomAttribute<WatchCompatibleModAttribute>() != null).ForEach(assembly =>
                {
                    try
                    {
                        Logging.Info($"Searching assembly {assembly.GetName().Name}");

                        var types = assembly.GetTypes();
                        types.Where(page => page.GetCustomAttribute<WatchCustomPageAttribute>() != null).ForEach(RegisterScreen);
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Exception thrown when searching assembly {assembly.GetName().Name}");
                        Logging.Error(ex);
                    }
                });
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

            Sounds = [];
            Sounds.Add("FriendJoin", await AssetLoader.LoadAsset<AudioClip>("Friend1"));
            Sounds.Add("FriendLeave", await AssetLoader.LoadAsset<AudioClip>("Friend2"));
            Sounds.Add("Press", await AssetLoader.LoadAsset<AudioClip>("Click"));

            var sprite_assets = AssetLoader.Bundle.LoadAssetWithSubAssets<Sprite>("Sheet");
            Sprites = Array.FindAll(sprite_assets, sprite => Enum.IsDefined(typeof(EDefaultSymbol), sprite.name)).ToDictionary(sprite => (EDefaultSymbol)Enum.Parse(typeof(EDefaultSymbol), sprite.name), sprite => sprite);

            // Objects

            WatchAsset = await AssetLoader.LoadAsset<GameObject>("Watch25");
            watch = Instantiate(WatchAsset);
            watch.name = "InfoWatch";
            var watch_component = watch.AddComponent<Watch>();
            watch_component.Rig = GorillaTagger.Instance.offlineVRRig;

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            string timeZone = (timeZoneInfo.SupportsDaylightSavingTime && timeZoneInfo.IsDaylightSavingTime(DateTime.Now)) ? timeZoneInfo.DaylightName : timeZoneInfo.StandardName;
            watch_component.TimeZone = timeZone;
            NetworkHandler.Instance.SetProperty("TimeZone", timeZone);

            float timeOffset = (float)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            watch_component.TimeOffset = timeOffset;
            NetworkHandler.Instance.SetProperty("TimeOffset", timeOffset);

            menu = Instantiate(await AssetLoader.LoadAsset<GameObject>("Menu"));
            menu.name = "InfoMenu";
            var menu_component = menu.AddComponent<Panel>();
            menu_component.Origin = watch.transform.Find("Point");
            //menu_component.Head = Player.Instance.headCollider.transform;

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

            WatchTrigger watchTrigger = watch.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();
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

            //Events.OnPlayerJoined += OnPlayerJoined;
            //Events.OnPlayerLeft += OnPlayerLeft;
            PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;

            HomePage.SetEntries(ScreenRegistry);
            Logging.Info("Switching to home");
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

        /*
        public void OnPlayerJoined(NetPlayer player, VRRig rig)
        {
            if (FriendLib.IsFriend(player.UserId))
            {
                string player_name = ((bool)rig && !string.IsNullOrEmpty(rig.playerNameVisible) && !string.IsNullOrWhiteSpace(rig.playerNameVisible)) ? rig.playerNameVisible : player.NickName.NormalizeName();
                watch.GetComponent<WatchModel>().Notify($"<size=60%>A FRIEND HAS JOINED:</size>\n<b><color=#{ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour)}>{player_name}</color></b>", 4, true);
            }
        }

        public void OnPlayerLeft(NetPlayer player, VRRig rig)
        {
            if (FriendLib.IsFriend(player.UserId))
            {
                string player_name = ((bool)rig && !string.IsNullOrEmpty(rig.playerNameVisible) && !string.IsNullOrWhiteSpace(rig.playerNameVisible)) ? rig.playerNameVisible : player.NickName.NormalizeName();
                watch.GetComponent<WatchModel>().Notify($"<size=60%>A FRIEND HAS LEFT:</size>\n<b><color=#{ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour)}>{player_name}</color></b>", 4, false);
            }
        }
        */

        public void PressButton(Button button, bool isLeftHand)
        {
            if (button)
            {
                AudioSource handSource = isLeftHand
                    ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                    : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : Sounds["Press"];
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

                AudioClip clip = Sounds["Press"];
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
