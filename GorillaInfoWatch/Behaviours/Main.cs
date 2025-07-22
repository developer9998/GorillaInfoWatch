using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GFriends = GorillaFriends.Main;
using InfoWatchScreen = GorillaInfoWatch.Models.InfoWatchScreen;
using PushButton = GorillaInfoWatch.Behaviours.UI.PushButton;
using Random = UnityEngine.Random;
using SnapSlider = GorillaInfoWatch.Behaviours.UI.SnapSlider;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }
        public static bool HasInstance => Instance is not null && (bool)Instance;

        // Pages

        private HomeScreen Home;

        private WarningScreen Warning;

        private InboxScreen Inbox;

        private GameObject screenObject;

        private readonly Dictionary<Type, InfoWatchScreen> registry = [];

        public InfoWatchScreen CurrentScreen;
        private readonly List<InfoWatchScreen> history = [];

        private readonly List<Notification> notifications = [];

        private InfoWatchData data;

        private FigureSignificance[] figures;

        private ItemSignificance[] cosmetics;

        private PlayerSignificance watch, verified;

        // Assets
        public GameObject WatchAsset;
        private GameObject localWatchObject, menu;

        private InfoWatch localInfoWatch;
        private Panel localPanel;

        // Menu
        private Image menu_background;
        private TMP_Text menu_header;
        private TMP_Text menu_description;
        private Transform menu_line_tform;
        internal List<WatchLine> menu_lines;

        // Display
        private TMP_Text menu_page_text;
        private PushButton button_prev_page, button_next_page, button_return_screen, button_reload_screen, buttonOpenInbox;

        public static Dictionary<InfoWatchSymbol, Sprite> Sprites;

        public static Dictionary<InfoWatchSound, AudioClip> Sounds = [];

        public static Dictionary<NetPlayer, PlayerSignificance> Significance = [];

        public async void Awake()
        {
            if (HasInstance && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            enabled = false;

            // Screens

            screenObject = new GameObject("ScreenContainer");
            screenObject.SetActive(false);
            screenObject.transform.SetParent(transform);

            List<Type> builtinPages =
            [
                typeof(ScoreboardScreen),
                typeof(PlayerInfoPage),
                typeof(DetailsScreen),
                typeof(FriendScreen),
                typeof(ModListPage),
                typeof(ModInfoPage),
                typeof(CreditScreen)
            ];

            builtinPages.ForEach(page => RegisterScreen(page));

            Type baseScreen = typeof(InfoWatchScreen);

            try
            {
                List<Assembly> assemblies = [];

                foreach (var pluginInfo in Chainloader.PluginInfos.Values)
                {
                    try
                    {
                        if (pluginInfo.Metadata?.GUID == Constants.GUID) continue;

                        if (pluginInfo.Instance is BaseUnityPlugin plugin && plugin.GetType()?.Assembly is Assembly assembly)
                        {
                            assemblies.Add(assembly);
                        }
                    }
                    catch
                    {
                        // TODO: write literally anything here
                        // "I guess you got better things, better things to do"
                    }
                }

                foreach (Assembly assembly in assemblies.Distinct())
                {
                    string assemblyName = assembly.GetName().Name;

                    if (assembly.GetCustomAttribute<InfoWatchCompatibleAttribute>() == null)
                    {
                        //Logging.Warning("Assembly missing InfoWatchCompatibleAttribute, which is probably okay, not every mod has to be compatible!");
                        continue;
                    }

                    Logging.Info(assemblyName);

                    List<Type> screenTypes = [];

                    try
                    {
                        Type[] assemblyTypes = assembly.GetTypes();

                        foreach (Type type in assemblyTypes)
                        {
                            if (type is null) continue;

                            try
                            {
                                if (type.IsSubclassOf(baseScreen) && baseScreen.IsAssignableFrom(type))
                                {
                                    screenTypes.Add(type);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Fatal($"Type {type.Name} could not be filtered");
                                Logging.Error(ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Assembly {assemblyName} likely had one or more types that failed to load");
                        Logging.Error(ex);
                        continue;
                    }

                    if (screenTypes.Count == 0)
                    {
                        Logging.Warning("Assembly contains no types valid for registration");
                        continue;
                    }

                    foreach (Type type in screenTypes)
                    {
                        if (RegisterScreen(type)) continue;
                        Logging.Warning($"Type {type.Name} could not be registered as a screen");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("Exception thrown when performing initial assembly search");
                Logging.Error(ex);
            }

            Home = GetScreen<HomeScreen>();
            Warning = GetScreen<WarningScreen>();
            Inbox = GetScreen<InboxScreen>();

            // Assets

            data = await AssetLoader.LoadAsset<InfoWatchData>("Data");

            figures = Array.ConvertAll(data.Figures, figure => (FigureSignificance)figure);
            cosmetics = Array.ConvertAll(data.Cosmetics, item => (ItemSignificance)item);
            watch = new("GorillaInfoWatch User", InfoWatchSymbol.InfoWatch);
            verified = new("Verified", InfoWatchSymbol.Verified);

            // "enumWeNeverReallyKnewEachOtherAnyway" is a play on words
            // "Enid, we never really knew each other anyway" - https://youtu.be/28oxinZmrW0

            foreach (InfoWatchSound enumWeNeverReallyKnewEachOtherAnyway in Enum.GetValues(typeof(InfoWatchSound)).Cast<InfoWatchSound>())
            {
                AudioClip clip = await AssetLoader.LoadAsset<AudioClip>(enumWeNeverReallyKnewEachOtherAnyway.ToString());
                if (clip)
                {
                    Sounds.Add(enumWeNeverReallyKnewEachOtherAnyway, clip);
                    continue;
                }
                Logging.Warning($"Missing AudioClip asset for sound: {enumWeNeverReallyKnewEachOtherAnyway}");
            }

            Sprite[] spriteArray = await AssetLoader.LoadAssetsWithSubAssets<Sprite>("Sheet");
            Sprites = Array.FindAll(spriteArray, sprite => Enum.IsDefined(typeof(InfoWatchSymbol), sprite.name)).ToDictionary(sprite => (InfoWatchSymbol)Enum.Parse(typeof(InfoWatchSymbol), sprite.name), sprite => sprite);

            // Objects

            WatchAsset = data.WatchPrefab;
            WatchAsset.SetActive(false);
            localWatchObject = Instantiate(WatchAsset);
            localWatchObject.name = "InfoWatch";

            localInfoWatch = localWatchObject.GetComponent<InfoWatch>();
            localInfoWatch.Rig = GorillaTagger.Instance.offlineVRRig;
            localWatchObject.SetActive(true);

            float timeOffset = (float)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            localInfoWatch.TimeOffset = timeOffset;
            NetworkManager.Instance.SetProperty("TimeOffset", timeOffset);

            // The majority of the remaining code in this method was written at a point when my main computer broke down
            // I wasn't able to work on C# as much as before, and most of my coding was done in Python in a robotics class in high school
            // Hence the amount of underscores used every five lines, because that's how it was done in Python I suppose

            menu = Instantiate(data.MenuPrefab);
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
                var line_component = line_tform.gameObject.AddComponent<WatchLine>();
                menu_lines.Add(line_component);
            }

            await new WaitForEndOfFrame().YieldAsync();
            await Task.Yield();

            // Components

            WatchTrigger watchTrigger = localWatchObject.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();
            watchTrigger.Menu = menu;

            menu_page_text = menu.transform.Find("Canvas/Page Text").GetComponent<TMP_Text>();

            button_next_page = menu.transform.Find("Canvas/Button_Next").AddComponent<PushButton>();
            button_next_page.OnButtonPressed = () =>
            {
                CurrentScreen.Section++;
                RefreshScreen();
            };

            button_prev_page = menu.transform.Find("Canvas/Button_Previous").AddComponent<PushButton>();
            button_prev_page.OnButtonPressed = () =>
            {
                CurrentScreen.Section--;
                RefreshScreen();
            };

            button_return_screen = menu.transform.Find("Canvas/Button_Return").AddComponent<PushButton>();
            button_return_screen.OnButtonPressed = () =>
            {
                SwitchScreen(history.Last());
            };

            button_reload_screen = menu.transform.Find("Canvas/Button_Redraw").AddComponent<PushButton>();
            button_reload_screen.OnButtonPressed = () =>
            {
                if (CurrentScreen is InfoWatchScreen screen)
                {
                    menu_lines.ForEach(line => line.Build(new ScreenLine("", []), true));
                    screen.OnRefresh();
                    screen.Content = screen.GetContent();
                    RefreshScreen();
                }
            };

            buttonOpenInbox ??= menu.transform.Find("Canvas/Button_Inbox").AddComponent<PushButton>();
            buttonOpenInbox.OnButtonPressed = delegate ()
            {
                SwitchScreen(Inbox);
            };

            localPanel = menu.AddComponent<Panel>();
            localPanel.Origin = localWatchObject.transform.Find("Point");

            Events.OnNotificationSent = OnNotificationSent;
            Events.OnNotificationOpened = OnNotifcationOpened;

            RoomSystem.JoinedRoomEvent += OnJoinedRoom;
            NetworkSystem.Instance.OnPlayerJoined += OnPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;

            Events.OnQuestCompleted += OnQuestCompleted;
            Events.OnGetUserCosmetics += OnGetUserCosmetics;

            screenObject.SetActive(true);
            Home.SetEntries([.. registry.Values]);
            SwitchScreen(Home);

            enabled = true;
        }

        public void OnJoinedRoom()
        {
            NetworkSystem.Instance.PlayerListOthers.ForEach(player => CheckPlayer(player));
        }

        public void OnPlayerJoined(NetPlayer player)
        {
            CheckPlayer(player);

            if (player.IsLocal) // called for the local player when marked "InGame" / connected to a room
                return;

            string userId = player.UserId;

            if (GFriends.IsFriend(userId))
            {
                Events.SendNotification(new("Your friend has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrFriend), player.GetNameRef().EnforceLength(12)), 3f, InfoWatchSound.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.GetNameRef().EnforceLength(12)}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
                return;
            }

            if (GFriends.IsVerified(userId))
            {
                Events.SendNotification(new("A verified user has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrVerified), player.GetNameRef().EnforceLength(12)), 3f, InfoWatchSound.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.GetNameRef().EnforceLength(12)}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
                return;
            }

            if (Significance.TryGetValue(player, out PlayerSignificance significance) && significance is FigureSignificance)
            {
                Events.SendNotification(new("A notable user has joined", player.GetNameRef().EnforceLength(12), 3f, InfoWatchSound.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.NickName.SanitizeName()}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
            }
        }

        public void OnPlayerLeft(NetPlayer player)
        {
            string userId = player.UserId;

            if (GFriends.IsFriend(userId))
            {
                Events.SendNotification(new("Your friend has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrFriend), player.GetNameRef().EnforceLength(12)), 5, InfoWatchSound.notificationNegative));
            }
            else if (GFriends.IsVerified(userId))
            {
                Events.SendNotification(new("A verified user has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrVerified), player.GetNameRef().EnforceLength(12)), 5, InfoWatchSound.notificationNegative));
            }
            else if (Significance.TryGetValue(player, out PlayerSignificance significance) && significance is FigureSignificance)
            {
                Events.SendNotification(new("A notable user has left", player.GetNameRef().EnforceLength(12), 5, InfoWatchSound.notificationNegative));
            }

            CheckPlayer(player);
        }

        public void OnQuestCompleted(RotatingQuestsManager.RotatingQuest quest)
        {
            Logging.Info($"Quest completed: {quest.GetTextDescription()}");
            Events.SendNotification(new("You completed a quest", quest.questName, 5, InfoWatchSound.notificationNeutral));
        }

        public void OnGetUserCosmetics(VRRig rig)
        {
            if (rig.Creator is not NetPlayer player || player.IsNull || player.IsLocal)
                return;

            if (CheckPlayer(player) && Significance.TryGetValue(player, out var significance) && significance is ItemSignificance item)
            {
                string userId = player.UserId;
                string displayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(item.ItemId));
                Events.SendNotification(new($"A notable cosmetic was detected", displayName, 5, InfoWatchSound.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.NickName.SanitizeName()}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
            }
        }

        public void SwitchScreen(InfoWatchScreen newScreen)
        {
            if (CurrentScreen is InfoWatchScreen lastScreen)
            {
                CurrentScreen.ScreenSwitchEvent -= SwitchScreen;
                CurrentScreen.UpdateScreenEvent -= RefreshScreen;

                CurrentScreen.enabled = false;
                CurrentScreen.Content = null;

                CurrentScreen.OnClose();

                if (history.Count == 0 || history.Last() != newScreen) history.Add(CurrentScreen);
            }

            CurrentScreen = newScreen;

            if (newScreen == Home) history.Clear();
            else if (history.Count > 0 && history.Last() == newScreen) history.RemoveAt(history.Count - 1);

            newScreen.ScreenSwitchEvent += SwitchScreen;
            newScreen.UpdateScreenEvent += delegate (bool includeWidgets)
            {
                newScreen.Content = newScreen.GetContent();
                RefreshScreen(includeWidgets);
            };

            newScreen.enabled = true;
            newScreen.OnShow();

            newScreen.Content = newScreen.GetContent();
            RefreshScreen();
        }

        public void SwitchScreen(Type type)
        {
            if (type is null) SwitchScreen(Home);
            else if (GetScreen(type, false) is InfoWatchScreen screen) SwitchScreen(screen);
        }

        public void RefreshScreen(bool includeWidgets = true)
        {
            buttonOpenInbox.gameObject.SetActive(CurrentScreen == Home);
            button_return_screen.gameObject.SetActive(CurrentScreen != Home);

            CurrentScreen.Content ??= CurrentScreen.GetContent();

            try
            {
                int sectionCount = 0;

                try
                {
                    sectionCount = CurrentScreen.Content.GetSectionCount();
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section count method threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                bool hasSection = sectionCount > 0;
                int currentSection = hasSection ? CurrentScreen.Section.Wrap(0, sectionCount) : 0;
                CurrentScreen.Section = currentSection;

                bool multiSection = hasSection && sectionCount > 1;
                button_next_page.gameObject.SetActive(multiSection);
                button_prev_page.gameObject.SetActive(multiSection);

                menu_page_text.text = multiSection ? $"{currentSection + 1}/{sectionCount}" : string.Empty;

                string sectionTitle = null;

                try
                {
                    sectionTitle = CurrentScreen.Content.GetTitleOfSection(currentSection);
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section title method threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                menu_header.text = $"{CurrentScreen.Title}{(string.IsNullOrEmpty(sectionTitle) ? "" : $" - {sectionTitle}")}";

                string description = null;

                try
                {
                    description = CurrentScreen.Description;
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section description property threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                if (string.IsNullOrEmpty(description) && menu_description.gameObject.activeSelf)
                {
                    menu_description.gameObject.SetActive(false);
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    menu_description.gameObject.SetActive(true);
                    menu_description.text = CurrentScreen.Description;
                }

                IEnumerable<ScreenLine> lines = [];

                try
                {
                    lines = CurrentScreen.Content.GetLinesAtSection(CurrentScreen.Section);
                }
                catch (Exception ex)
                {
                    lines = Enumerable.Repeat<ScreenLine>(new("Line is null!"), Constants.SectionCapacity);

                    Logging.Fatal("Screen section lines method threw exception");
                    Logging.Error(ex);
                }

                for (int i = 0; i < menu_lines.Count; i++)
                {
                    WatchLine menuLine = menu_lines[i];

                    if (i >= Constants.SectionCapacity) Logging.Warning($"{i} >= {Constants.SectionCapacity}");

                    if (lines.ElementAtOrDefault(i) is ScreenLine screenLine)
                    {
                        bool wasLineActive = menuLine.gameObject.activeSelf;
                        menuLine.gameObject.SetActive(true);
                        menuLine.Build(screenLine, !wasLineActive || includeWidgets);
                        continue;
                    }

                    menuLine.gameObject.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Displaying screen contents of {CurrentScreen.GetType().Name} threw exception");
                Logging.Error(ex);

                PlayErrorSound();
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
                PlayErrorSound();
                return false;
            }

            if (!baseScreen.IsAssignableFrom(type))
            {
                Logging.Warning("Type is not assignable from screen");
                PlayErrorSound();
                return false;
            }

            Component component = screenObject.AddComponent(type);

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
            PlayErrorSound();

            return false;
        }

        public T GetScreen<T>(bool registerFallback = true) where T : InfoWatchScreen => (T)GetScreen(typeof(T), registerFallback);
        public InfoWatchScreen GetScreen(Type type, bool registerFallback = true) => (registry.ContainsKey(type) || (registerFallback && RegisterScreen(type))) ? registry[type] : null;

        public void OnNotificationSent(Notification notification)
        {
            if (notification is null || notification.Opened || notifications.Contains(notification))
                return;

            notifications.Add(notification);

            if (Sounds.TryGetValue(notification.Sound, out AudioClip audio))
                localInfoWatch.audioDevice.PlayOneShot(audio);

            GorillaTagger.Instance.StartVibration(localInfoWatch.InLeftHand, 0.04f, 0.2f);

            var stateMachine = localInfoWatch.stateMachine;
            Menu_StateBase currentState = stateMachine.CurrentState is Menu_SubState subState ? subState.previousState : stateMachine.CurrentState;
            stateMachine.SwitchState(new Menu_Notification(localInfoWatch, currentState, notification));

            RefreshInbox();
        }

        public void OnNotifcationOpened(Notification notification, bool digest)
        {
            if (notification is null || notification.Opened || !notifications.Contains(notification) || notification.Processing)
            {
                Logging.Warning($"OpenNotification \"{(notification is not null ? notification.DisplayText : "Null")}\"");
                if (notification is not null)
                {
                    Logging.Warning($"Processing: {notification.Processing}");
                    Logging.Warning($"Opened: {notification.Opened}");
                    Logging.Warning($"Known: {notifications.Contains(notification)}");
                    if (Inbox.Notifications.Contains(notification))
                    {
                        Logging.Info("Exists in inbox, correcting error");
                        RefreshInbox();
                    }
                }
                return;
            }

            notification.Opened = true;

            if (digest && notification.Screen is not null && GetScreen(notification.Screen.ScreenType) is InfoWatchScreen screen)
            {
                try
                {
                    if (notification.Screen.Task is Task task)
                    {
                        Task.Run(async delegate ()
                        {
                            await task;
                            notification.Processing = false;
                            SwitchScreen(screen);
                        });
                    }
                    else SwitchScreen(screen);
                }
                catch (Exception ex)
                {
                    Logging.Fatal($"Unable to open notification \"{notification.DisplayText}\"");
                    Logging.Error(ex);
                    PlayErrorSound();
                }
            }

            RefreshInbox();
        }

        public void RefreshInbox()
        {
            Inbox.Notifications = [.. notifications.Where(notif => !notif.Opened).OrderByDescending(notif => notif.Created)];

            if (CurrentScreen == Inbox) Inbox.SetContent();

            localInfoWatch.home?.RefreshBell(Inbox.Notifications.Count);
        }

        public void PlayErrorSound()
        {
            // Random.Range (inclusive, exclusive)
            // meaning Random.Range(1, 7) can give you 1, 2, 3, 4, 5, and 6
            if (Enum.TryParse(string.Concat("error", Random.Range(1, 7)), out InfoWatchSound sound) && Sounds.TryGetValue(sound, out AudioClip audioClip))
                localInfoWatch.audioDevice.PlayOneShot(audioClip);
        }

        public bool CheckPlayer(NetPlayer player)
        {
            if (player is null || player.IsNull || (!player.IsLocal && !player.InRoom))
                return false;

            PlayerSignificance predicate = null;

            if (Array.Find(figures, figure => figure.IsValid(player)) is FigureSignificance figure)
                predicate = figure;
            else if (player.IsLocal || GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig playerRig) && playerRig.TryGetComponent(out NetworkedPlayer component) && component.HasInfoWatch)
                predicate = watch;
            else if (Array.Find(cosmetics, cosmetic => cosmetic.IsValid(player)) is ItemSignificance cosmetic)
                predicate = cosmetic;
            else if (GFriends.IsVerified(player.UserId))
                predicate = verified;

            if (predicate is not null)
            {
                if (!Significance.ContainsKey(player))
                {
                    Logging.Info($"Added significant player {player.NickName}: {predicate.Symbol}");
                    Significance.Add(player, predicate);
                    Events.OnSignificanceChanged?.SafeInvoke(player, predicate);
                    return true;
                }
                if (Significance[player] != predicate)
                {
                    Logging.Info($"Changed significant player {player.NickName}: from {Significance[player].Symbol} to {predicate.Symbol}");
                    Significance[player] = predicate;
                    Events.OnSignificanceChanged?.SafeInvoke(player, predicate);
                    return true;
                }
            }
            else if (Significance.ContainsKey(player))
            {
                Logging.Info($"Removed significant player {player.NickName}");
                Significance.Remove(player);
                Events.OnSignificanceChanged?.SafeInvoke(player, null);
            }

            return false;
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

                AudioClip clip = Sounds[InfoWatchSound.widgetSlider];
                handSource.PlayOneShot(clip, 0.2f);
            }
        }

        public void PressSwitch(Switch button, bool isLeftHand)
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