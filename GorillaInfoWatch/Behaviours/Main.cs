using BepInEx.Bootstrap;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Pages;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GorillaInfoWatch.Models;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : Singleton<Main>
    {
        // Pages

        private HomePage HomePage;

        private ModRoomWarningPage WarnPage;
        
        public List<WatchScreen> ScreenRegistry = [];

        // private Dictionary<Type, Screen> ScreenCache = [];
        
        public WatchScreen CurrentScreen;

        private List<WatchScreen> screen_history = [];

        // Assets
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

        public Dictionary<string, AudioClip> Sounds;

        public Dictionary<EDefaultSymbol, Sprite> Sprites;

        public async override void Initialize()
        {
            enabled = false;

            FriendLib.InitializeLib(Chainloader.PluginInfos);

            // Screens

            var builtinPages = new List<Type>()
            {
                typeof(ScoreboardScreen),
                typeof(DetailsScreen),
                typeof(FriendScreen),
                typeof(ModStatusPage),
                typeof(CreditScreen)
            };

            builtinPages.ForEach(RegisterPage);

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                assemblies.Where(assembly => assembly != null && assembly.GetCustomAttribute<WatchCompatibleModAttribute>() != null).ForEach(assembly =>
                {
                    try
                    {
                        Logging.Info($"Searching assembly {assembly.GetName().Name}");

                        var types = assembly.GetTypes();
                        types.Where(page => page.GetCustomAttribute<WatchCustomPageAttribute>() != null).ForEach(RegisterPage);
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

            HomePage = gameObject.AddComponent<HomePage>();
            WarnPage = gameObject.AddComponent<ModRoomWarningPage>();

            // Assets

            Sounds = [];
            Sounds.Add("FriendJoin", await AssetLoader.LoadAsset<AudioClip>("Friend1"));
            Sounds.Add("FriendLeave", await AssetLoader.LoadAsset<AudioClip>("Friend2"));
            Sounds.Add("Press", await AssetLoader.LoadAsset<AudioClip>("Click"));

            var sprite_assets = AssetLoader.Bundle.LoadAssetWithSubAssets<Sprite>("Sheet");
            Sprites = Array.FindAll(sprite_assets, sprite => Enum.IsDefined(typeof(EDefaultSymbol), sprite.name)).ToDictionary(sprite => (EDefaultSymbol)Enum.Parse(typeof(EDefaultSymbol), sprite.name), sprite => sprite);

            // Objects

            watch = Instantiate(await AssetLoader.LoadAsset<GameObject>("Watch25"));
            var watch_component = watch.AddComponent<Watch>();
            watch_component.Rig = GorillaTagger.Instance.offlineVRRig;
            watch_component.TimeOffset = (float)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalSeconds;

            menu = Instantiate(await AssetLoader.LoadAsset<GameObject>("Menu"));
            menu.transform.SetParent(watch.transform.Find("Point"));
            menu.transform.localPosition = Vector3.zero;
            menu.transform.localEulerAngles = Vector3.zero;
            menu.AddComponent<Panel>();

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
                if (CurrentScreen != null)
                {
                    CurrentScreen.OnScreenOpen();
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

        public void SwitchScreen(WatchScreen screen)
        {
            if (CurrentScreen != null)
            {
                Logging.Warning(CurrentScreen.GetType().Name);

                CurrentScreen.RequestScreenSwitch -= SwitchScreen;
                CurrentScreen.RequestDisplayScreen -= DisplayScreen;
                CurrentScreen.RequestSetLines -= DisplayScreen;
                CurrentScreen.enabled = false;
                CurrentScreen.OnScreenClose();

                if (screen.GetType() != typeof(HomePage))
                {
                    screen_history.Add(CurrentScreen);
                }
            }

            Logging.Info(screen.GetType().Name);
            CurrentScreen = screen;

            if (screen_history.Count > 0 && screen_history[^1] == screen)
            {
                screen_history.RemoveAt(screen_history.Count - 1);
            }

            screen.enabled = true;
            screen.RequestScreenSwitch += SwitchScreen;
            screen.RequestDisplayScreen += DisplayScreen;
            screen.RequestSetLines += DisplayScreen;
            screen.OnScreenOpen();

            DisplayScreen();
            button_return_screen.gameObject.SetActive(screen_history.Count > 0);
        }

        public void SwitchScreen(Type type)
        {
            if (TryGetComponent(type, out Component component) && component is WatchScreen screen)
            {
                SwitchScreen(screen);
                return;
            }
            RegisterPage(type);
            SwitchScreen(ScreenRegistry[^1]);
        }

        public void DisplayScreen() => DisplayScreen(false);

        public void DisplayScreen(bool text_exclusive)
        {
            Logging.Info($"Only text: {text_exclusive}");

            var content = CurrentScreen.Content;

            int page_count = content.GetPageCount();
            CurrentScreen.PageNumber = CurrentScreen.PageNumber.Wrap(0, page_count);

            button_next_page.gameObject.SetActive(page_count > 1);
            button_prev_page.gameObject.SetActive(page_count > 1);
            menu_page_text.text = (button_next_page.gameObject.activeSelf || button_prev_page.gameObject.activeSelf) ? $"{CurrentScreen.PageNumber + 1}/{page_count}" : "";

            string page_title = content.GetPageTitle(CurrentScreen.PageNumber);
            menu_header.text = $"{CurrentScreen.Title}{(string.IsNullOrEmpty(page_title) ? "": $" - {page_title}")}";
            if (string.IsNullOrEmpty(CurrentScreen.Description) && menu_description.gameObject.activeSelf)
            {
                menu_description.gameObject.SetActive(false);
            }
            else if (!string.IsNullOrEmpty(CurrentScreen.Description))
            {
                menu_description.gameObject.SetActive(true);
                menu_description.text = CurrentScreen.Description;
            }

            var lines = content.GetPageLines(CurrentScreen.PageNumber);
            
            for (int i = 0; i < Constants.LinesPerPage; i++)
            {
                var menu_line = menu_lines[i];
                var page_line = lines.ElementAtOrDefault(i);

                if (page_line != null)
                {
                    bool set_widgets = !menu_line.gameObject.activeSelf;
                    menu_line.gameObject.SetActive(true);
                    menu_line.Build(page_line, set_widgets || !text_exclusive);
                }
                else
                {
                    menu_line.gameObject.SetActive(false);
                }
            }
        }

        public void RegisterPage(Type type)
        {
            try
            {
                var component = gameObject.AddComponent(type);
                if (component is WatchScreen page)
                {
                    page.enabled = false;
                    RegisterPage(page);
                }
                else
                {
                    Destroy(component);
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Exception thrown when casting Page from Type {type.FullName}");
                Logging.Error(ex);
            }
        }

        public void RegisterPage(WatchScreen page)
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
                    Metadata.AddItem("Tags", Metadata.GetItem("Tags", 0) + 1);
                }
            }
        }
    }
}
