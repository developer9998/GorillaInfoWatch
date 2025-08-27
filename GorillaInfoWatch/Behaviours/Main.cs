using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PushButton = GorillaInfoWatch.Behaviours.UI.PushButton;
using Random = UnityEngine.Random;
using Screen = GorillaInfoWatch.Models.Screen;
using SnapSlider = GorillaInfoWatch.Behaviours.UI.SnapSlider;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : MonoBehaviourPunCallbacks
    {
        public static Main Instance { get; private set; }

        public static ModContent Content { get; private set; }

        public static ReadOnlyDictionary<Sounds, AudioClip> EnumToAudio { get; private set; }
        public static ReadOnlyDictionary<Symbols, Sprite> EnumToSprite { get; private set; }

        public static ReadOnlyCollection<FigureSignificance> Significance_Figures { get; private set; }
        public static ReadOnlyCollection<ItemSignificance> Significance_Cosmetics { get; private set; }

        public static PlayerSignificance Significance_Watch { get; private set; }
        public static PlayerSignificance Significance_Verified { get; private set; }

        public static Screen ActiveScreen { get; private set; }

        // Pages

        private HomeScreen Home;

        private WarningScreen Warning;

        private InboxScreen Inbox;

        private GameObject screenObject;

        private readonly Dictionary<Type, Screen> registry = [];

        private readonly List<Screen> history = [];

        private readonly List<Notification> notifications = [];

        // Assets
        private GameObject localWatchObject, menu;

        private InfoWatch localInfoWatch;
        private Panel localPanel;

        // Menu
        private Image menuBackground;
        private TMP_Text menuHeader;
        private TMP_Text menuDescription;
        private Transform menuLinesOrigin;
        internal List<WatchLine> menuLines;

        // Display
        private TMP_Text menu_page_text;
        private PushButton button_prev_page, button_next_page, button_return_screen, button_reload_screen, buttonOpenInbox;

        private bool wasRoomAllowed = true;

        public async void Awake()
        {
            if (Instance != null && Instance != this)
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
                typeof(PlayerInspectorScreen),
                typeof(RoomInspectorPage),
                typeof(DetailsScreen),
                typeof(FriendScreen),
                typeof(ModListPage),
                typeof(ModInfoPage),
                typeof(CreditScreen)
            ];

            builtinPages.ForEach(page => RegisterScreen(page));

            Type screenType = typeof(Screen);

            try
            {
                List<Assembly> assemblies = [];

                foreach (PluginInfo pluginInfo in Chainloader.PluginInfos.Values)
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
                    Logging.Info(assemblyName);

                    if (assembly.GetCustomAttribute<InfoWatchCompatibleAttribute>() == null)
                    {
                        //Logging.Warning("Assembly missing InfoWatchCompatibleAttribute, which is probably okay, not every mod has to be compatible!");
                        continue;
                    }

                    List<Type> screenTypes = [];

                    try
                    {
                        Type[] assemblyTypes = assembly.GetTypes();

                        foreach (Type type in assemblyTypes)
                        {
                            if (type is null) continue;

                            try
                            {
                                if (type.IsSubclassOf(screenType) && screenType.IsAssignableFrom(type))
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

            Content = await AssetLoader.LoadAsset<ModContent>("Data");

            Significance_Figures = Array.AsReadOnly(Array.ConvertAll(Content.Figures, figure => (FigureSignificance)figure));
            Significance_Cosmetics = Array.AsReadOnly(Array.ConvertAll(Content.Cosmetics, item => (ItemSignificance)item));
            Significance_Watch = new("InfoWatch User", Symbols.InfoWatch);
            Significance_Verified = new("Verified", Symbols.Verified);

            Dictionary<Sounds, AudioClip> audioClipDictionary = [];

            foreach (Sounds enumeration in Enum.GetValues(typeof(Sounds)).Cast<Sounds>())
            {
                if (enumeration == Sounds.none) continue;

                AudioClip clip = await AssetLoader.LoadAsset<AudioClip>(enumeration.GetName());

                if (clip is not null)
                {
                    audioClipDictionary.Add(enumeration, clip);
                    continue;
                }

                Logging.Warning($"Missing AudioClip asset for sound: {enumeration.GetName()}");
            }

            EnumToAudio = new ReadOnlyDictionary<Sounds, AudioClip>(audioClipDictionary);

            Sprite[] spriteCollection = await AssetLoader.LoadAssetsWithSubAssets<Sprite>("Sheet");
            EnumToSprite = new ReadOnlyDictionary<Symbols, Sprite>(Array.FindAll(spriteCollection, sprite => Enum.IsDefined(typeof(Symbols), sprite.name)).ToDictionary(sprite => (Symbols)Enum.Parse(typeof(Symbols), sprite.name), sprite => sprite));

            // Objects

            Content.WatchPrefab.SetActive(false);
            localWatchObject = Instantiate(Content.WatchPrefab);
            localWatchObject.name = "InfoWatch";

            localInfoWatch = localWatchObject.GetComponent<InfoWatch>();
            localInfoWatch.Rig = GorillaTagger.Instance.offlineVRRig;
            localWatchObject.SetActive(true);

            float timeOffset = Convert.ToSingle(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes);
            localInfoWatch.TimeOffset = timeOffset;
            NetworkManager.Instance.SetProperty("TimeOffset", timeOffset);

            menu = Instantiate(Content.MenuPrefab);
            menu.name = "InfoMenu";
            menu.transform.SetParent(transform);
            menu.gameObject.SetActive(true);

            menuBackground = menu.transform.Find("Canvas/Background").GetComponent<Image>();
            menuHeader = menu.transform.Find("Canvas/Header/Title").GetComponent<TMP_Text>();
            menuDescription = menu.transform.Find("Canvas/Header/Description").GetComponent<TMP_Text>();
            menuLinesOrigin = menu.transform.Find("Canvas/Lines");
            menuLines = new(Constants.SectionCapacity);

            foreach (Transform transform in menuLinesOrigin)
            {
                transform.gameObject.SetActive(true);

                if (!transform.TryGetComponent(out WatchLine lineComponent))
                    lineComponent = transform.gameObject.AddComponent<WatchLine>();

                menuLines.Add(lineComponent);
            }

            await new WaitForEndOfFrame().AsAwaitable();
            await Task.Yield();

            // Components

            menu_page_text = menu.transform.Find("Canvas/Page Text").GetComponent<TMP_Text>();

            button_next_page = menu.transform.Find("Canvas/Button_Next").AddComponent<PushButton>();
            button_next_page.OnButtonPressed = () =>
            {
                ActiveScreen.Section++;
                RefreshScreen();
            };

            button_prev_page = menu.transform.Find("Canvas/Button_Previous").AddComponent<PushButton>();
            button_prev_page.OnButtonPressed = () =>
            {
                ActiveScreen.Section--;
                RefreshScreen();
            };

            button_return_screen = menu.transform.Find("Canvas/Button_Return").AddComponent<PushButton>();
            button_return_screen.OnButtonPressed = () =>
            {
                Screen overrideScreen = null;

                if (ActiveScreen is Screen screen)
                {
                    PropertyInfo returnTypeProperty = null;

                    try
                    {
                        returnTypeProperty = AccessTools.Property(screen.GetType(), nameof(Screen.ReturnType));
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal("ReturnType property could not be accessed for return button process");
                        Logging.Error(ex);
                    }

                    overrideScreen = (returnTypeProperty != null && returnTypeProperty.DeclaringType == screen.GetType()) ? GetScreen((Type)returnTypeProperty.GetValue(screen)) : null;
                }

                SwitchScreen(overrideScreen ?? history.Last());
            };

            button_reload_screen = menu.transform.Find("Canvas/Button_Redraw").AddComponent<PushButton>();
            button_reload_screen.OnButtonPressed = () =>
            {
                if (ActiveScreen is Screen screen)
                {
                    menuLines.ForEach(line => line.Build(new InfoLine("", []), true));
                    screen.OnRefresh();
                    screen.Lines = screen.GetContent();
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

            Trigger watchTrigger = localWatchObject.transform.Find("Hand Model/Trigger").gameObject.AddComponent<Trigger>();
            watchTrigger.Menu = localPanel;

            screenObject.SetActive(true);
            Home.SetEntries([.. registry.Values]);
            SwitchScreen(Home);

            Notifications.RequestSendNotification = HandleSentNotification;
            Notifications.RequestOpenNotification = HandleOpenNotification;

            enabled = true;
        }

        public void Update()
        {
            if (GorillaComputer.hasInstance)
            {
                GorillaComputer computer = GorillaComputer.instance;
                bool roomNotAllowed = computer.roomNotAllowed;

                if (wasRoomAllowed != roomNotAllowed) return;
                wasRoomAllowed = !roomNotAllowed;

                if (roomNotAllowed) Notifications.SendNotification(new("Room join failure", "Room inaccessible", 3, Sounds.notificationNegative));
            }
        }

        public void HandleSentNotification(Notification notification)
        {
            if (notification is null || notification.Opened || notifications.Contains(notification))
                return;

            notifications.Add(notification);

            if (EnumToAudio.TryGetValue(notification.Sound, out AudioClip audioClip))
                localInfoWatch.audioDevice.PlayOneShot(audioClip);

            bool isSilent = notification.Sound == Sounds.none;
            GorillaTagger.Instance.StartVibration(localInfoWatch.InLeftHand, isSilent ? 0.06f : 0.04f, isSilent ? 0.1f : 0.2f);

            var stateMachine = localInfoWatch.stateMachine;
            Menu_StateBase currentState = stateMachine.CurrentState is Menu_SubState subState ? subState.previousState : stateMachine.CurrentState;
            stateMachine.SwitchState(new Menu_Notification(localInfoWatch, currentState, notification));

            RefreshInbox();
        }

        public void HandleOpenNotification(Notification notification, bool process)
        {
            if (notification is null || notification.Opened || !notifications.Contains(notification) || notification.Processing)
            {
                Logging.Warning($"OpenNotification \"{(notification is not null ? notification.DisplayText : "Null")}\"");

                if (notification is not null)
                {
                    Logging.Warning($"Processing: {notification.Processing}");
                    Logging.Warning($"Opened: {notification.Opened}");
                    Logging.Warning($"Known: {notifications.Contains(notification)}");
                    if (Inbox.Contents.Contains(notification))
                    {
                        Logging.Info("Exists in inbox, correcting error");
                        RefreshInbox();
                    }
                }

                return;
            }

            notification.Opened = true;

            if (process && notification.Screen is not null && GetScreen(notification.Screen.ScreenType) is Screen screen)
            {
                try
                {
                    if (notification.Screen.Task is Task task)
                    {
                        ThreadingHelper.Instance.StartSyncInvoke(async () =>
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
            Inbox.Contents = [.. notifications.Where(notif => !notif.Opened).OrderByDescending(notif => notif.Created)];

            if (ActiveScreen == Inbox) Inbox.SetContent();

            localInfoWatch.home?.RefreshBell(Inbox.Contents.Count);
        }

        public void PlayErrorSound()
        {
            // Random.Range (inclusive, exclusive)
            // meaning Random.Range(1, 7) can give you 1, 2, 3, 4, 5, and 6
            if (Enum.TryParse(string.Concat("error", Random.Range(1, 7)), out Sounds sound) && EnumToAudio.TryGetValue(sound, out AudioClip audioClip))
                localInfoWatch.audioDevice.PlayOneShot(audioClip);
        }

        public void PressButton(PushButton button, bool isLeftHand)
        {
            if (button)
            {
                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : EnumToAudio[Sounds.widgetButton];
                AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(isLeftHand);
                handPlayer.PlayOneShot(clip, 0.2f);
            }
        }

        public void PressSlider(SnapSlider slider, bool isLeftHand)
        {
            if (slider)
            {
                AudioClip clip = EnumToAudio[Sounds.widgetSlider];
                AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(isLeftHand);
                handPlayer.PlayOneShot(clip, 0.2f);
            }
        }

        public void PressSwitch(Switch button, bool isLeftHand)
        {
            if (button)
            {
                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : EnumToAudio[Sounds.widgetSwitch];
                AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(isLeftHand);
                handPlayer.PlayOneShot(clip, 0.2f);
            }
        }

        public T GetScreen<T>(bool registerFallback = true) where T : Screen => (T)GetScreen(typeof(T), registerFallback);

        public Screen GetScreen(Type type, bool registerFallback = true) => (registry.ContainsKey(type) || (registerFallback && RegisterScreen(type))) ? registry[type] : null;

        public void SwitchScreen(Screen newScreen)
        {
            if (ActiveScreen is Screen lastScreen)
            {
                ActiveScreen.ScreenSwitchEvent -= SwitchScreen;
                ActiveScreen.UpdateScreenEvent -= RefreshScreen;

                ActiveScreen.enabled = false;
                ActiveScreen.Lines = null;

                ActiveScreen.OnClose();

                if (history.Count == 0 || history.Last() != newScreen) history.Add(ActiveScreen);
            }

            ActiveScreen = newScreen;

            if (newScreen == Home) history.Clear();
            else if (history.Count > 0 && history.Last() == newScreen) history.RemoveAt(history.Count - 1);

            newScreen.ScreenSwitchEvent += SwitchScreen;
            newScreen.UpdateScreenEvent += delegate (bool includeWidgets)
            {
                newScreen.Lines = newScreen.GetContent();
                RefreshScreen(includeWidgets);
            };

            newScreen.enabled = true;
            newScreen.OnShow();

            newScreen.Lines = newScreen.GetContent();
            RefreshScreen();
        }

        public void SwitchScreen(Type type)
        {
            if (type is null) SwitchScreen(Home);
            else if (GetScreen(type, false) is Screen screen) SwitchScreen(screen);
        }

        public void RefreshScreen(bool includeWidgets = true)
        {
            bool onHomeScreen = ActiveScreen == Home;

            PropertyInfo returnTypeProperty = null;

            try
            {
                returnTypeProperty = AccessTools.Property(ActiveScreen.GetType(), nameof(Screen.ReturnType));
            }
            catch (Exception ex)
            {
                Logging.Fatal("ReturnType property could not be accessed for screen process");
                Logging.Error(ex);
            }

            bool considerReturnType = returnTypeProperty != null && returnTypeProperty.GetValue(ActiveScreen) != null && returnTypeProperty.DeclaringType == ActiveScreen.GetType();

            buttonOpenInbox.gameObject.SetActive(onHomeScreen);
            button_return_screen.gameObject.SetActive(!onHomeScreen && (history.Count > 0 || considerReturnType));

            ActiveScreen.Lines ??= ActiveScreen.GetContent();

            try
            {
                int sectionCount = 0;

                try
                {
                    sectionCount = ActiveScreen.Lines.SectionCount;
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section count property getter threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                bool hasSection = sectionCount > 0;
                int currentSection = hasSection ? Mathf.Clamp(ActiveScreen.Section, 0, sectionCount) : 0;
                ActiveScreen.Section = currentSection;

                bool multiSection = hasSection && sectionCount > 1;
                button_next_page.gameObject.SetActive(multiSection);
                button_prev_page.gameObject.SetActive(multiSection);
                menu_page_text.text = multiSection ? $"{currentSection + 1}/{sectionCount}" : string.Empty;

                string sectionTitle = null;

                try
                {
                    sectionTitle = ActiveScreen.Lines.GetTitleOfSection(currentSection);
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section title method threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                menuHeader.text = $"{ActiveScreen.Title}{((string.IsNullOrEmpty(sectionTitle) || string.IsNullOrWhiteSpace(sectionTitle)) ? "" : $": {sectionTitle}")}";

                string description = null;

                try
                {
                    description = ActiveScreen.Description;
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section description property threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                bool hasDescription = !string.IsNullOrEmpty(description) && !string.IsNullOrWhiteSpace(description);

                if (!hasDescription && menuDescription.gameObject.activeSelf)
                {
                    menuDescription.gameObject.SetActive(false);
                }
                else if (hasDescription)
                {
                    menuDescription.gameObject.SetActive(true);
                    menuDescription.text = ActiveScreen.Description;
                }

                IEnumerable<InfoLine> lines = [];

                try
                {
                    lines = ActiveScreen.Lines.GetLinesAtSection(ActiveScreen.Section);
                }
                catch (Exception ex)
                {
                    lines = Enumerable.Repeat<InfoLine>(new("Line is null!"), Constants.SectionCapacity);

                    Logging.Fatal("Screen section lines method threw exception");
                    Logging.Error(ex);
                }

                for (int i = 0; i < menuLines.Count; i++)
                {
                    WatchLine menuLine = menuLines[i];

                    if (i >= Constants.SectionCapacity) Logging.Warning($"{i} >= {Constants.SectionCapacity}");

                    if (lines.ElementAtOrDefault(i) is InfoLine screenLine)
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
                Logging.Fatal($"Displaying screen contents of {ActiveScreen.GetType().Name} threw exception");
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

            Type baseScreen = typeof(Screen);

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

            if (component is Screen screen)
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

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Logging.Error("OnJoinRoomFailed");
            Logging.Message($"{returnCode}: {message}");

            switch (returnCode)
            {
                case ErrorCode.GameFull:
                    Notifications.SendNotification(new("Room join failure", "Room is full", 3, Sounds.notificationNegative));
                    break;
            }
        }

        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
            Logging.Fatal("OnCustomAuthenticationFailed");
            Logging.Message(debugMessage);

            Notifications.SendNotification(new("Photon PUN failure", "Custom Auth failed", 3, Sounds.notificationNegative));
        }
    }
}