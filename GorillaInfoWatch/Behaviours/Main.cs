using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Shortcuts;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using InfoScreen = GorillaInfoWatch.Models.InfoScreen;
using PushButton = GorillaInfoWatch.Behaviours.UI.PushButton;
using Random = UnityEngine.Random;
using SnapSlider = GorillaInfoWatch.Behaviours.UI.SnapSlider;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : MonoBehaviourPunCallbacks
    {
        public static Main Instance { get; private set; }

        // Content
        public static ModContent Content { get; private set; }

        // Enum/Unity Object dictionaries
        public static ReadOnlyDictionary<Sounds, AudioClip> EnumToAudio { get; private set; }
        public static ReadOnlyDictionary<Symbols, Sprite> EnumToSprite { get; private set; }

        // Significance
        public static ReadOnlyCollection<FigureSignificance> Significance_Figures { get; private set; }
        public static ReadOnlyCollection<ItemSignificance> Significance_Cosmetics { get; private set; }
        public static PlayerSignificance Significance_Watch { get; private set; }
        public static PlayerSignificance Significance_Verified { get; private set; }
        public static PlayerSignificance Significance_Master { get; private set; }
        public static PlayerSignificance Significance_Friend { get; private set; }
        public static PlayerSignificance Significance_RecentlyPlayed { get; private set; }

        // Screens
        public static InfoScreen ActiveScreen { get; private set; }

        // Screens

        private HomeScreen Home;

        private WarningScreen Warning;

        private InboxScreen Inbox;

        private GameObject screenObject;

        private readonly Dictionary<Type, InfoScreen> screenCache = [];

        private readonly List<Notification> notifications = [];

        // Functions



        // Assets

        private AssetLoader _assetLoader;

        private GameObject localWatchObject, menu;

        private Watch localInfoWatch;
        private Panel localPanel;

        // Menu

        private Image menuBackground;
        private TMP_Text menuHeader;
        private TMP_Text menuDescription;
        private Transform menuLinesOrigin;
        private List<PanelLine> _panelLines;
        private TMP_Text _sampleLineText;

        // Display

        private TMP_Text menuPageText;
        private PushButton menuPrevPageButton, menuNextPageButton, menuReturnButton, menuReloadButton, menuInboxButton;

        // Runtime

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

            // This list should be expanded to encapsulate proper screens (in the form of their types)
            // "Proper" screens refer to all but any hardcoded screens (including Home, Inbox, and Warning screens)
            List<Type> includedScreenTypes =
            [
                typeof(ScoreboardScreen),
                typeof(RoomInspectorScreen),
                typeof(PlayerInspectorScreen),
                typeof(DetailsScreen),
                typeof(FriendScreen),
                typeof(ModListScreen),
                typeof(ShortcutListScreen),
                typeof(ModInspectorScreen),
                typeof(CreditScreen)
            ];

            includedScreenTypes.ForEach(page => RegisterScreen(page));

            List<Type> includedShortcutsTypes =
            [
                typeof(ShortcutRegistrar_Rooms)
            ];

            includedShortcutsTypes.ForEach(funcReg => RegisterShortcuts(funcReg));

            Type screenType = typeof(InfoScreen);

            Type shortcutRegType = typeof(ShortcutRegistrar);

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
                    string assemblyName;

                    try
                    {
                        assemblyName = assembly.GetName().Name;
                        Logging.Info(assemblyName);

                        if (assembly.GetCustomAttribute<InfoWatchCompatibleAttribute>() == null)
                        {
                            //Logging.Warning("Assembly missing InfoWatchCompatibleAttribute, which is probably okay, not every mod has to be compatible!");
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    List<Type> screenTypes = [], shortcutRegTypes = [];

                    try
                    {
                        Type[] assemblyTypes = assembly.GetTypes();

                        foreach (Type type in assemblyTypes)
                        {
                            if (type is null) continue;

                            if (type.IsSubclassOf(screenType) && screenType.IsAssignableFrom(type)) screenTypes.Add(type);
                            else if (type.IsSubclassOf(shortcutRegType) && shortcutRegType.IsAssignableFrom(type)) shortcutRegTypes.Add(type);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Assembly {assemblyName} likely had one or more types that failed to load");
                        Logging.Error(ex);
                        continue;
                    }

                    screenTypes.ForEach(type => RegisterScreen(type));
                    shortcutRegTypes.ForEach(type => RegisterShortcuts(type));
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("Exception thrown when performing initial assembly search");
                Logging.Error(ex);
            }

            // Hardcoded screens are retrieved here, ensure registerFallback parameter is turned set to True for each method
            Home = GetScreen<HomeScreen>();
            Warning = GetScreen<WarningScreen>();
            Inbox = GetScreen<InboxScreen>();

            GetScreen<ShortcutListScreen>().SetEntries(ShortcutRegistrar.Shortcuts);

            _assetLoader = new AssetLoader("GorillaInfoWatch.Content.watchbundle");

            Content = await _assetLoader.LoadAsset<ModContent>("Data");

            Significance_Figures = Array.AsReadOnly(Array.ConvertAll(Content.Figures, figure => (FigureSignificance)figure));
            Significance_Cosmetics = Array.AsReadOnly(Array.ConvertAll(Content.Cosmetics, item => (ItemSignificance)item));
            Significance_Watch = new("GorillaInfoWatch User", Symbols.InfoWatch, $"{Constants.SignificancePlayerNameTag} is a user of GorillaInfoWatch");
            Significance_Verified = new("Verified", Symbols.Verified, $"{Constants.SignificancePlayerNameTag} is marked as verified by the GorillaFriends mod");
            Significance_Master = new("Master Client", Symbols.None, $"{Constants.SignificancePlayerNameTag} is the master client of the room, responsible for hosting the game mode manager");
            Significance_Friend = new("Friend", Symbols.None, $"You have {Constants.SignificancePlayerNameTag} added as a friend using the GorillaFriends mod");
            Significance_RecentlyPlayed = new("Recently Played", Symbols.None, $"You have played with {Constants.SignificancePlayerNameTag} recently, according to the GorillaFriends mod");

            Dictionary<Sounds, AudioClip> audioClipDictionary = [];

            foreach (Sounds enumeration in Enum.GetValues(typeof(Sounds)).Cast<Sounds>())
            {
                if (enumeration == Sounds.None) continue;

                AudioClip clip = await _assetLoader.LoadAsset<AudioClip>(enumeration.GetName());

                if (clip is not null)
                {
                    audioClipDictionary.Add(enumeration, clip);
                    continue;
                }

                Logging.Warning($"Missing AudioClip asset for sound: {enumeration.GetName()}");
            }

            EnumToAudio = new ReadOnlyDictionary<Sounds, AudioClip>(audioClipDictionary);

            Sprite[] spriteCollection = await _assetLoader.LoadAssetsWithSubAssets<Sprite>("Sheet");
            EnumToSprite = new ReadOnlyDictionary<Symbols, Sprite>(Array.FindAll(spriteCollection, sprite => Enum.IsDefined(typeof(Symbols), sprite.name)).ToDictionary(sprite => (Symbols)Enum.Parse(typeof(Symbols), sprite.name), sprite => sprite));

            // Objects

            Content.WatchPrefab.SetActive(false);
            localWatchObject = Instantiate(Content.WatchPrefab);
            localWatchObject.name = "InfoWatch";

            localInfoWatch = localWatchObject.GetComponent<Watch>();
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
            _panelLines = new(Constants.SectionCapacity);

            foreach (Transform transform in menuLinesOrigin)
            {
                transform.gameObject.SetActive(true);

                if (!transform.TryGetComponent(out PanelLine lineComponent))
                    lineComponent = transform.gameObject.AddComponent<PanelLine>();

                _panelLines.Add(lineComponent);
            }

            PanelLine firstLine = _panelLines[0];
            GameObject textObject = Instantiate(firstLine.Text.gameObject, firstLine.GetComponentInParent<Canvas>(true).transform);
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localRotation = Quaternion.identity;
            _sampleLineText = textObject.GetComponent<TMP_Text>();
            _sampleLineText.textWrappingMode = TextWrappingModes.Normal;
            _sampleLineText.enableAutoSizing = false;
            _sampleLineText.fontSize = _sampleLineText.fontSizeMax;
            _sampleLineText.color = Color.clear;

            await new WaitForEndOfFrame().AsAwaitable();
            await Task.Yield();

            // Components

            menuPageText = menu.transform.Find("Canvas/Page Text").GetComponent<TMP_Text>();

            menuNextPageButton = menu.transform.Find("Canvas/Button_Next").AddComponent<PushButton>();
            menuNextPageButton.OnButtonPressed = () =>
            {
                ActiveScreen.sectionNumber++;
                UpdateScreen();
            };

            menuPrevPageButton = menu.transform.Find("Canvas/Button_Previous").AddComponent<PushButton>();
            menuPrevPageButton.OnButtonPressed = () =>
            {
                ActiveScreen.sectionNumber--;
                UpdateScreen();
            };

            menuReturnButton = menu.transform.Find("Canvas/Button_Return").AddComponent<PushButton>();
            menuReturnButton.OnButtonPressed = () =>
            {
                if (ActiveScreen is not InfoScreen activeScreen) return;

                if (activeScreen.GetType().GetCustomAttribute<ShowOnHomeScreenAttribute>() is ShowOnHomeScreenAttribute attribute)
                {
                    LoadScreen(Home);
                    return;
                }

                PropertyInfo returnTypeProperty = AccessTools.Property(activeScreen.GetType(), nameof(InfoScreen.ReturnType));

                if (returnTypeProperty != null)
                {
                    object property = null;

                    try
                    {
                        property = returnTypeProperty.GetValue(activeScreen);
                    }
                    catch (Exception ex)
                    {
                        property = null;
                        Logging.Error(ex);
                    }

                    if (property != null && property is Type type && GetScreen(type, false) is InfoScreen screen)
                    {
                        LoadScreen(screen);
                        return;
                    }
                }

                LoadScreen(activeScreen.CallerScreenType);
            };

            menuReloadButton = menu.transform.Find("Canvas/Button_Redraw").AddComponent<PushButton>();
            menuReloadButton.OnButtonPressed = () =>
            {
                if (ActiveScreen is InfoScreen screen)
                {
                    _panelLines.ForEach(line => line.Build(new InfoLine("", []), true));
                    screen.OnScreenReload();
                    screen.contents = screen.GetContent();
                    UpdateScreen();
                }
            };

            menuInboxButton ??= menu.transform.Find("Canvas/Button_Inbox").AddComponent<PushButton>();
            menuInboxButton.OnButtonPressed = delegate ()
            {
                LoadScreen(Inbox);
            };

            localPanel = menu.AddComponent<Panel>();
            localPanel.Origin = localWatchObject.transform.Find("Point");

            Trigger watchTrigger = localWatchObject.transform.Find("Hand Model/Trigger").gameObject.AddComponent<Trigger>();
            watchTrigger.Menu = localPanel;

            screenObject.SetActive(true);
            Home.SetEntries([.. screenCache.Values]);
            LoadScreen(Home);
            RefreshInbox();

            Events.OnQuestCompleted += OnQuestCompleted;
            Notifications.SendRequest += HandleSentNotification;
            Notifications.OpenRequest += HandleOpenNotification;

            MothershipClientApiUnity.OnMessageNotificationSocket += OnMothershipMessageRecieved;

            enabled = true;

            Events.OnModInitialized?.Invoke();
        }

        public void Update()
        {
            if (GorillaComputer.hasInstance)
            {
                GorillaComputer computer = GorillaComputer.instance;
                bool roomNotAllowed = computer.roomNotAllowed;

                if (wasRoomAllowed != roomNotAllowed) return;
                wasRoomAllowed = !roomNotAllowed;

                if (roomNotAllowed) Notifications.SendNotification(new("Room Join failure", "Room inaccessible", 3, Sounds.notificationNegative));
            }
        }

        public void HandleSentNotification(Notification notification)
        {
            if (notification is null || notification.Opened || notifications.Contains(notification))
                return;

            notifications.Add(notification);

            if (EnumToAudio.TryGetValue(notification.Sound, out AudioClip audioClip))
                localInfoWatch.audioDevice.PlayOneShot(audioClip);

            bool isSilent = notification.Sound == Sounds.None;
            GorillaTagger.Instance.StartVibration(localInfoWatch.InLeftHand, isSilent ? 0.2f : 0.04f, isSilent ? 0.1f : 0.2f);

            var stateMachine = localInfoWatch.MenuStateMachine;
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

            if (process && notification.Screen is not null && GetScreen(notification.Screen.ScreenType) is InfoScreen screen)
            {
                try
                {
                    if (notification.Screen.Task is Task task)
                    {
                        ThreadingHelper.Instance.StartSyncInvoke(async () =>
                        {
                            await task;
                            notification.Processing = false;
                            LoadScreen(screen);
                        });
                    }
                    else LoadScreen(screen);
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

            localInfoWatch.HomeState?.UpdateBell(Inbox.Contents.Count);
        }

        public void PlayErrorSound()
        {
            string soundName = string.Concat("error", Random.Range(1, 7));
            if (Enum.TryParse(soundName, out Sounds sound) && EnumToAudio.TryGetValue(sound, out AudioClip audioClip)) localInfoWatch.audioDevice.PlayOneShot(audioClip);
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

        public T GetScreen<T>(bool registerFallback = true) where T : InfoScreen => (T)GetScreen(typeof(T), registerFallback);

        public InfoScreen GetScreen(Type type, bool registerFallback = true) => (screenCache.ContainsKey(type) || (registerFallback && RegisterScreen(type))) ? screenCache[type] : null;

        public void LoadScreen(InfoScreen newScreen)
        {
            Type callerScreenType = null;

            if (ActiveScreen is InfoScreen lastScreen)
            {
                lastScreen.LoadScreenRequest -= LoadScreen;
                lastScreen.UpdateScreenRequest -= UpdateScreen;

                lastScreen.enabled = false;

                lastScreen.OnScreenUnload();

                PreserveScreenSectionAttribute preserveSection = lastScreen.GetType().GetCustomAttribute<PreserveScreenSectionAttribute>();
                if (preserveSection == null) lastScreen.sectionNumber = 0;
                if (preserveSection == null || preserveSection.ClearContent) lastScreen.contents = null;

                if (newScreen != Home && lastScreen.CallerScreenType == newScreen.GetType()) callerScreenType = newScreen.CallerScreenType;
                else if (newScreen != Home) callerScreenType = lastScreen.GetType();
            }

            ActiveScreen = newScreen;

            newScreen.CallerScreenType = callerScreenType;

            newScreen.LoadScreenRequest += LoadScreen;
            newScreen.UpdateScreenRequest += delegate (bool includeWidgets)
            {
                newScreen.contents = newScreen.GetContent();
                UpdateScreen(includeWidgets);
            };

            newScreen.enabled = true;
            newScreen.OnScreenLoad();

            newScreen.contents = newScreen.GetContent();
            UpdateScreen();
        }

        public void LoadScreen(Type type)
        {
            if (type is null)
            {
                if (ActiveScreen != Home) menuReturnButton?.OnButtonPressed?.Invoke();
                return;
            }
            else if (GetScreen(type, false) is InfoScreen screen) LoadScreen(screen);
        }

        public void UpdateScreen(bool includeWidgets = true)
        {
            bool onHomeScreen = ActiveScreen == Home;

            PropertyInfo returnTypeProperty = null;

            try
            {
                returnTypeProperty = AccessTools.Property(ActiveScreen.GetType(), nameof(InfoScreen.ReturnType));
            }
            catch (Exception ex)
            {
                Logging.Fatal("ReturnType property could not be accessed for screen process");
                Logging.Error(ex);
                PlayErrorSound();
            }

            bool considerReturnType = returnTypeProperty != null && returnTypeProperty.GetValue(ActiveScreen) != null;

            menuInboxButton.gameObject.SetActive(onHomeScreen);
            menuReturnButton.gameObject.SetActive(!onHomeScreen && (ActiveScreen.CallerScreenType != null || considerReturnType));

            ActiveScreen.contents ??= ActiveScreen.GetContent();

            try
            {
                int sectionCount = 0;

                try
                {
                    sectionCount = ActiveScreen.contents.SectionCount;
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section count property getter threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                bool hasSection = sectionCount > 0;
                int currentSection = hasSection ? MathExtensions.Wrap(ActiveScreen.sectionNumber, 0, sectionCount) : 0;
                ActiveScreen.sectionNumber = currentSection;

                bool multiSection = hasSection && sectionCount > 1;
                menuNextPageButton.gameObject.SetActive(multiSection);
                menuPrevPageButton.gameObject.SetActive(multiSection);
                menuPageText.text = multiSection ? $"{currentSection + 1}/{sectionCount}" : string.Empty;

                string sectionTitle = null;

                try
                {
                    sectionTitle = ActiveScreen.contents.GetTitleOfSection(currentSection);
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Screen section title method threw exception");
                    Logging.Error(ex);
                    PlayErrorSound();
                }

                menuHeader.text = string.Concat(ActiveScreen.Title, (!string.IsNullOrEmpty(sectionTitle) && !string.IsNullOrWhiteSpace(sectionTitle)) ? $" : {sectionTitle}" : string.Empty);

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

                List<InfoLine> lines = [];

                try
                {
                    lines = [.. ActiveScreen.contents.GetLinesAtSection(ActiveScreen.sectionNumber)];
                }
                catch (Exception ex)
                {
                    lines = [.. Enumerable.Repeat<InfoLine>(new("Placeholder"), Constants.SectionCapacity)];

                    Logging.Fatal("Screen section lines method threw exception");
                    Logging.Error(ex);
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];

                    if (line.Restrictions.HasFlag(LineRestrictions.Wrapping))
                    {
                        int insertPosition = i;
                        lines.RemoveAt(insertPosition);

                        string[] textArray = _sampleLineText.GetArrayFromText(line.Text);
                        for (int j = textArray.Length - 1; j >= 0; j--)
                        {
                            lines.Insert(insertPosition, new InfoLine(textArray[j], j == 0 ? line.Widgets : []));
                        }

                        i += textArray.Length - 1;
                    }
                }

                for (int i = 0; i < _panelLines.Count; i++)
                {
                    PanelLine panelLine = _panelLines[i];

                    if (i >= Constants.SectionCapacity) Logging.Warning($"{i} >= {Constants.SectionCapacity}");

                    if (lines.ElementAtOrDefault(i) is InfoLine screenLine)
                    {
                        bool wasLineActive = panelLine.gameObject.activeSelf;
                        panelLine.gameObject.SetActive(true);

                        try
                        {
                            panelLine.Build(screenLine, !wasLineActive || includeWidgets);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error(ex);
                            PlayErrorSound();
                        }

                        continue;
                    }

                    panelLine.gameObject.SetActive(false);
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
            if (type == null) return false;

            Logging.Info($"RegisterScreen: {type.Name} of {type.Namespace}");

            if (screenCache.ContainsKey(type))
            {
                Logging.Warning("Registry contains key");
                return false;
            }

            Type screenType = typeof(InfoScreen);

            if (!type.IsSubclassOf(screenType))
            {
                Logging.Warning("Type is not subclass of screen");
                PlayErrorSound();
                return false;
            }

            if (!screenType.IsAssignableFrom(type))
            {
                Logging.Warning("Type is not assignable from screen");
                PlayErrorSound();
                return false;
            }

            Component component = screenObject.AddComponent(type);

            if (component is InfoScreen screen)
            {
                if (screenCache.ContainsValue(screen))
                {
                    Logging.Warning("Registry contains value (component of type)");
                    Destroy(component);
                    return false;
                }

                screen.enabled = false;

                screenCache[type] = screen;

                Logging.Info("Register success");
                return true;
            }

            Logging.Warning("Component of type is not screen (this shouldn't happen)");

            Destroy(component);
            PlayErrorSound();

            return false;
        }

        public bool RegisterShortcuts(Type type)
        {
            if (type == null) return false;

            Logging.Message($"RegisterFunctionRegistrar: {type.FullName}");

            Type registrarType = typeof(ShortcutRegistrar);

            if (!type.IsSubclassOf(registrarType))
            {
                PlayErrorSound();
                return false;
            }

            if (!registrarType.IsAssignableFrom(type))
            {
                PlayErrorSound();
                return false;
            }

            try
            {
                ShortcutRegistrar registrar = (ShortcutRegistrar)Activator.CreateInstance(type);
                Logging.Message("CreateInstance success");
            }
            catch (Exception ex)
            {
                Logging.Fatal("CreateInstance exception");
                Logging.Error(ex);

                return false;
            }

            return true;
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Logging.Fatal("OnJoinRoomFailed");
            Logging.Message($"{returnCode}: {message}");

            switch (returnCode)
            {
                case ErrorCode.GameFull:
                    Notifications.SendNotification(new("Room Join failure", "Room is full", 3, Sounds.notificationNegative));
                    break;
            }
        }

        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
            Logging.Fatal("OnCustomAuthenticationFailed");
            Logging.Message(debugMessage);

            Notifications.SendNotification(new("Photon PUN failure", "Custom Auth failed", 3, Sounds.notificationNegative));
        }

        public void OnQuestCompleted(RotatingQuest quest)
        {
            Logging.Info($"Quest Completed: {quest.GetTextDescription()}");

            Notifications.SendNotification(new("You completed a Quest", quest.questName, 5, Sounds.notificationNeutral));
        }

        public void OnMothershipMessageRecieved(NotificationsMessageResponse notification, nint _)
        {
            string title = notification.Title;
            string body = notification.Body;
            Logging.Message($"\"{title}\": \"{body}\"");

            string[] array;

            switch (title)
            {
                case "Warning":
                    array = body.Split('|');
                    if (array.Length != 2) break;

                    string warnCategory = array[0];
                    string[] warnReasons = array[1].Split(',');
                    if (warnReasons.Length == 0) break;

                    string warnReasonString = string.Join(", ", warnReasons.Select(reason => reason.ToTitleCase().Replace('_', ' ')));
                    Notifications.SendNotification(new($"{warnCategory.ToTitleCase()} warning received", warnReasonString, 3f + (warnReasons.Length * 0.333333f), Sounds.notificationNegative));

                    break;

                case "Mute":
                    array = body.Split('|');
                    if (array.Length != 3 || !array[0].Equals("voice", StringComparison.OrdinalIgnoreCase)) break;

                    if (array[2].Length > 0 && int.TryParse(array[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int muteDuration))
                    {
                        TimeSpan timeSpan = TimeSpan.FromSeconds(muteDuration);
                        Notifications.SendNotification(new("Voice mute sanction", $"{(timeSpan.TotalHours >= 1f ? $"{timeSpan.TotalHours} hour" : $"{timeSpan.TotalMinutes} minute")} mute", 5, Sounds.notificationNegative));
                    }
                    else
                    {
                        Notifications.SendNotification(new("Voice mute sanction", "Indefinite mute", 5, Sounds.notificationNegative));
                    }

                    break;
            }
        }
    }
}