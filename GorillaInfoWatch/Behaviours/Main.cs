using BepInEx;
using BepInEx.Bootstrap;
using GorillaExtensions;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Behaviours.UI.Widgets;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Interfaces;
using GorillaInfoWatch.Models.Shortcuts;
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using InfoScreen = GorillaInfoWatch.Models.InfoScreen;
using Object = UnityEngine.Object;
using PushButton = GorillaInfoWatch.Behaviours.UI.Widgets.PushButton;
using Random = UnityEngine.Random;
using SnapSlider = GorillaInfoWatch.Behaviours.UI.Widgets.SnapSlider;

namespace GorillaInfoWatch.Behaviours;

public class Main : MonoBehaviourPunCallbacks
{
    public static Main Instance { get; private set; }

    // Content

    public static ModContent Content { get; private set; }

    public static readonly Dictionary<Enum, Object> UnityObjectDictionary = [];

    // Screens

    private GameObject _screenRoot;

    private readonly Dictionary<Type, InfoScreen> _screens = [];
    private InfoScreen _loadedScreen;

    private HomeScreen _homeScreen;

    private InboxScreen _inboxScreen;
    private readonly List<Notification> _notifications = [];

    private readonly List<Type> includedScreenTypes =
    [
        typeof(ScoreboardScreen),
        typeof(RoomInspectorScreen),
        typeof(PlayerInspectorScreen),
        typeof(DetailsScreen),
        typeof(FriendScreen),
        typeof(ModListScreen),
        typeof(ModInspectorScreen),
        typeof(ShortcutListScreen),
        typeof(SettingsListScreen),
        typeof(SettingsInspectorScreen),
        typeof(CreditScreen),
        typeof(VersionWarningScreen)
    ];

    // Shortcuts

    private readonly Dictionary<Type, ShortcutRegistrar> _shortcutRegistars = [];

    private readonly List<Type> includedShortcutsTypes =
    [
        typeof(ShortcutRegistrar_Rooms)
    ];

    // Assets

    private AssetLoader _assetLoader;

    private GameObject _watchObject, _panelObject, _userInputObject;

    private Watch _infoWatch;
    private Panel _panel;
    private UserInput _userInput;

    // Panel

    private TMP_Text _panelTitle, _panelDescription, _panelPageText;
    private Image _panelBackground;

    private Transform _panelLinesRoot;
    private List<PanelLine> _panelLines;
    private TMP_Text _sampleLineText;

    private PushButton _panelPrevPageButton, _panelNextPageButton, _panelReturnButton, _panelReloadButton, _panelInboxButton;

    private readonly SectionLine _placeholderLine = new(string.Empty, []);

    // Runtime

    private bool _wasRoomAllowed = true;

    private readonly Dictionary<WatchInteractionSource, float> _interactionTime = [];

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

        _screenRoot = new GameObject("Screen Root");
        _screenRoot.SetActive(false);
        _screenRoot.transform.SetParent(transform);

        includedScreenTypes.ForEach(page => RegisterScreen(page));
        includedShortcutsTypes.ForEach(funcReg => RegisterShortcuts(funcReg));

        Type screenType = typeof(InfoScreen);

        Type shortcutRegType = typeof(ShortcutRegistrar);

        try
        {
            List<Assembly> assemblies = [];

            foreach (PluginInfo pluginInfo in Chainloader.PluginInfos.Values)
            {
                if (pluginInfo.Metadata?.GUID == Constants.GUID) continue;

                if (pluginInfo.Instance is BaseUnityPlugin plugin && plugin.GetType()?.Assembly is Assembly assembly)
                {
                    assemblies.Add(assembly);
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
                catch (Exception ex)
                {
                    Logging.Fatal("Assembly derived from plugin failed with retrieving basic data");
                    Logging.Error(ex);

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
        _homeScreen = GetScreen<HomeScreen>();
        _inboxScreen = GetScreen<InboxScreen>();

        _assetLoader = new AssetLoader("GorillaInfoWatch.Content.watchbundle");

        Content = await _assetLoader.LoadAsset<ModContent>("Data");

        foreach (Sounds sound in Enum.GetValues(typeof(Sounds)).Cast<Sounds>())
        {
            if (sound == Sounds.None) continue;
            AudioClip clip = await _assetLoader.LoadAsset<AudioClip>(sound.GetName());
            UnityObjectDictionary.Add(sound, clip);
        }

        Sprite[] spriteSheet = await _assetLoader.LoadAssetsWithSubAssets<Sprite>("Sheet");
        foreach (Symbols symbol in Enum.GetValues(typeof(Symbols)).Cast<Symbols>())
        {
            if (Array.Find(spriteSheet, sprite => sprite.name == symbol.GetName()) is not Sprite sprite) continue;
            UnityObjectDictionary.Add(symbol, sprite);
        }

        // Objects

        Content.WatchPrefab.SetActive(false);
        _watchObject = Instantiate(Content.WatchPrefab);
        _watchObject.name = "Watch";

        _infoWatch = _watchObject.GetComponent<Watch>();
        _infoWatch.Rig = GorillaTagger.Instance.offlineVRRig;
        _watchObject.SetActive(true);

        float timeOffset = Convert.ToSingle(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes);
        _infoWatch.TimeOffset = timeOffset;
        NetworkManager.Instance.SetProperty("TimeOffset", timeOffset);

        _panelObject = Instantiate(Content.MenuPrefab);
        _panelObject.name = "Panel";
        _panelObject.transform.SetParent(transform);
        _panelObject.gameObject.SetActive(true);

        _panelBackground = _panelObject.transform.Find("Canvas/Background").GetComponent<Image>();
        _panelTitle = _panelObject.transform.Find("Canvas/Header/TextContainer/Title").GetComponent<TMP_Text>();
        _panelDescription = _panelObject.transform.Find("Canvas/Header/TextContainer/Description").GetComponent<TMP_Text>();

        _panelLinesRoot = _panelObject.transform.Find("Canvas/Lines");
        _panelLines = new(Constants.SectionCapacity);

        foreach (Transform transform in _panelLinesRoot)
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

        _panelPageText = _panelObject.transform.Find("Canvas/Page Text").GetComponent<TMP_Text>();

        _panelNextPageButton = _panelObject.transform.Find("Canvas/Button_Next").AddComponent<PushButton>();
        _panelNextPageButton.OnButtonPressed = () =>
        {
            _loadedScreen.sectionNumber++;
            UpdateScreen();
        };

        _panelPrevPageButton = _panelObject.transform.Find("Canvas/Button_Previous").AddComponent<PushButton>();
        _panelPrevPageButton.OnButtonPressed = () =>
        {
            _loadedScreen.sectionNumber--;
            UpdateScreen();
        };

        _panelReturnButton = _panelObject.transform.Find("Canvas/Button_Return").AddComponent<PushButton>();
        _panelReturnButton.OnButtonPressed = () =>
        {
            if (_loadedScreen.GetType().GetCustomAttribute<ShowOnHomeScreenAttribute>() is ShowOnHomeScreenAttribute attribute)
            {
                LoadScreen(_homeScreen);
                return;
            }

            PropertyInfo returnTypeProperty = AccessTools.Property(_loadedScreen.GetType(), nameof(InfoScreen.ReturnType));

            if (returnTypeProperty != null)
            {
                object property = null;

                try
                {
                    property = returnTypeProperty.GetValue(_loadedScreen);
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

            LoadScreen(_loadedScreen.CallerScreenType);
        };

        _panelReloadButton = _panelObject.transform.Find("Canvas/Button_Redraw").AddComponent<PushButton>();
        _panelReloadButton.OnButtonPressed = () =>
        {
            _panelLines.ForEach(line => line.Build(_placeholderLine, true));
            _loadedScreen.OnScreenReload();
            _loadedScreen.content = _loadedScreen.GetContent();
            UpdateScreen();
        };

        _panelInboxButton ??= _panelObject.transform.Find("Canvas/Button_Inbox").AddComponent<PushButton>();
        _panelInboxButton.OnButtonPressed = () =>
        {
            LoadScreen(_inboxScreen);
        };

        _panel = _panelObject.AddComponent<Panel>();
        _panel.Origin = _watchObject.transform.Find("Point");

        Transform triggerTransform = _watchObject.transform.Find("Hand Model/Trigger");
        _panel.Trigger = triggerTransform;

        Trigger watchTrigger = triggerTransform.gameObject.AddComponent<Trigger>();
        watchTrigger.panel = _panel;

        _userInputObject = Instantiate(await _assetLoader.LoadAsset<GameObject>("UserInput"));
        _userInputObject.name = "User Input";
        _userInputObject.transform.SetParent(transform);

        _userInput = _userInputObject.AddComponent<UserInput>();
        _userInput.Panel = _panel;
        _userInputObject.SetActive(false);

        _screenRoot.SetActive(true);

        GetScreen<ShortcutListScreen>().SetEntries([.. _shortcutRegistars.Values]);

        _homeScreen.SetEntries([.. _screens.Values]);
        LoadScreen(_homeScreen);

#if RELEASE
        CheckVersion(result =>
        {
            if (!result.isOutdated) return;

            VersionWarningScreen.LatestVersion = result.latestVersion;
            LoadScreen(typeof(VersionWarningScreen));

            _panelReloadButton.gameObject.SetActive(false);
            _panelReturnButton.gameObject.SetActive(false);
            _panelInboxButton.gameObject.SetActive(false);
        });
#endif

        RefreshInbox();

        Events.OnQuestCompleted += OnQuestCompleted;
        Notifications.SendRequest += HandleSentNotification;
        Notifications.OpenRequest += HandleOpenNotification;
        MothershipClientApiUnity.OnMessageNotificationSocket += OnMothershipMessageRecieved;

        IInitializeCallback[] initializable = gameObject.GetComponents<IInitializeCallback>();
        initializable.ForEach(obj => obj.Initialize());

        enabled = true;
    }

    public void Update()
    {
        if (GorillaComputer.hasInstance)
        {
            GorillaComputer computer = GorillaComputer.instance;
            bool roomNotAllowed = computer.roomNotAllowed;

            if (_wasRoomAllowed != roomNotAllowed) return;
            _wasRoomAllowed = !roomNotAllowed;

            if (roomNotAllowed) Notifications.SendNotification(new("Room Join failure", "Room inaccessible", 3, Sounds.notificationNegative));
        }
    }

    #region Notification handling

    public void HandleSentNotification(Notification notification)
    {
        if (notification is null || notification.Opened || _notifications.Contains(notification) || _loadedScreen.GetType() == typeof(VersionWarningScreen))
            return;

        _notifications.Add(notification);

        if (notification.Sound.AsAudioClip() is AudioClip audioClip) _infoWatch.audioDevice.PlayOneShot(audioClip, Configuration.NotificationSoundVolume.Value);

        bool isSilent = notification.Sound == Sounds.None;
        GorillaTagger.Instance.StartVibration(_infoWatch.InLeftHand, isSilent ? Configuration.SilentNotifHapticAmplitude.Value : Configuration.NotifHapticAmplitude.Value, isSilent ? Configuration.SilentNotifHapticDuration.Value : Configuration.NotifHapticDuration.Value);

        var stateMachine = _infoWatch.MenuStateMachine;
        Menu_StateBase currentState = stateMachine.CurrentState is Menu_SubState subState ? subState.previousState : stateMachine.CurrentState;
        stateMachine.SwitchState(new Menu_Notification(_infoWatch, currentState, notification));

        RefreshInbox();
    }

    public void HandleOpenNotification(Notification notification, bool process)
    {
        if (notification is null || notification.Opened || !_notifications.Contains(notification) || notification.Processing)
        {
            Logging.Warning($"OpenNotification \"{(notification is not null ? notification.DisplayText : "Null")}\"");

            if (notification is not null)
            {
                Logging.Warning($"Processing: {notification.Processing}");
                Logging.Warning($"Opened: {notification.Opened}");
                Logging.Warning($"Known: {_notifications.Contains(notification)}");
                if (_inboxScreen.Contents.Contains(notification))
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
        _inboxScreen.Contents = [.. _notifications.Where(notif => !notif.Opened).OrderByDescending(notif => notif.Created)];

        if (_loadedScreen == _inboxScreen) _inboxScreen.SetContent();

        _infoWatch.HomeState?.UpdateBell(_inboxScreen.Contents.Count);
    }

    #endregion

    #region Screen utilities and handling

    public T GetScreen<T>(bool registerFallback = true) where T : InfoScreen => (T)GetScreen(typeof(T), registerFallback);

    public InfoScreen GetScreen(Type type, bool registerFallback = true) => (_screens.ContainsKey(type) || (registerFallback && RegisterScreen(type))) ? _screens[type] : null;

    public void LoadScreen(InfoScreen newScreen)
    {
        Type callerScreenType = null;

        if (_loadedScreen is InfoScreen lastScreen)
        {
            lastScreen.LoadScreenRequest -= LoadScreen;
            lastScreen.UpdateScreenRequest -= UpdateScreen;

            lastScreen.enabled = false;

            lastScreen.OnScreenUnload();

            PreserveScreenSectionAttribute preserveSection = lastScreen.GetType().GetCustomAttribute<PreserveScreenSectionAttribute>();
            if (preserveSection == null) lastScreen.sectionNumber = 0;
            if (preserveSection == null || preserveSection.ClearContent) lastScreen.content = null;

            if (newScreen != _homeScreen && lastScreen.CallerScreenType == newScreen.GetType()) callerScreenType = newScreen.CallerScreenType;
            else if (newScreen != _homeScreen) callerScreenType = lastScreen.GetType();
        }

        _loadedScreen = newScreen;
        InfoScreen.LoadedScreen = newScreen;

        newScreen.CallerScreenType = callerScreenType;

        newScreen.LoadScreenRequest += LoadScreen;
        newScreen.UpdateScreenRequest += delegate (bool includeWidgets)
        {
            newScreen.content = newScreen.GetContent();
            UpdateScreen(includeWidgets);
        };

        newScreen.enabled = true;
        newScreen.OnScreenLoad();

        newScreen.content = newScreen.GetContent();
        UpdateScreen();
    }

    public void LoadScreen(Type type)
    {
        if (type is null)
        {
            if (_loadedScreen != _homeScreen) _panelReturnButton?.OnButtonPressed?.Invoke();
            return;
        }

        if (GetScreen(type, false) is InfoScreen screen) LoadScreen(screen);
    }

    private void UpdateScreen(bool includeWidgets = true)
    {
        bool onHomeScreen = _loadedScreen == _homeScreen;

        PropertyInfo returnTypeProperty = null;

        try
        {
            returnTypeProperty = AccessTools.Property(_loadedScreen.GetType(), nameof(InfoScreen.ReturnType));
        }
        catch (Exception ex)
        {
            Logging.Fatal("ReturnType property could not be accessed for screen process");
            Logging.Error(ex);
            PlayErrorSound();
        }

        bool considerReturnType = returnTypeProperty != null && returnTypeProperty.GetValue(_loadedScreen) != null;

        _panelInboxButton.gameObject.SetActive(onHomeScreen);
        _panelReturnButton.gameObject.SetActive(!onHomeScreen && (_loadedScreen.CallerScreenType != null || considerReturnType));

        _loadedScreen.content ??= _loadedScreen.GetContent();

        try
        {
            int sectionCount = 0;

            try
            {
                sectionCount = _loadedScreen.content.SectionCount;
            }
            catch (Exception ex)
            {
                Logging.Fatal("Screen section count property getter threw exception");
                Logging.Error(ex);
                PlayErrorSound();
            }

            bool hasSection = sectionCount > 0;
            int sectionNumber = hasSection ? MathExtensions.Wrap(_loadedScreen.sectionNumber, 0, sectionCount) : 0;
            _loadedScreen.sectionNumber = sectionNumber;

            bool multiSection = hasSection && sectionCount > 1;
            _panelNextPageButton.gameObject.SetActive(multiSection);
            _panelPrevPageButton.gameObject.SetActive(multiSection);
            _panelPageText.text = multiSection ? $"{sectionNumber + 1}/{sectionCount}" : string.Empty;

            Section section;

            try
            {
                section = _loadedScreen.content.GetSection(sectionNumber);
            }
            catch (Exception ex)
            {
                section = new(title: "Placeholder", lines: Enumerable.Repeat(_placeholderLine, Constants.SectionCapacity));

                Logging.Fatal($"{_loadedScreen.content.GetType().Name} of {_loadedScreen.content.GetType().Namespace} not could construct section at {sectionNumber}");
                Logging.Error(ex);
            }

            string sectionTitle = section.Definition.Title;

            _panelTitle.text = string.Concat(_loadedScreen.Title, (sectionTitle != null && sectionTitle.Length > 0) ? $" <color=#808080>></color> {sectionTitle}" : string.Empty);

            string description = null;

            try
            {
                description = _loadedScreen.Description;
            }
            catch (Exception ex)
            {
                Logging.Fatal("Screen section description property threw exception");
                Logging.Error(ex);
                PlayErrorSound();
            }

            string sectionDescription = section.Definition.Description;

            if (sectionDescription != null && sectionDescription.Length > 0) description = sectionDescription;

            bool hasDescription = description != null && description.Length > 0;

            if (!hasDescription && _panelDescription.gameObject.activeSelf)
            {
                _panelDescription.gameObject.SetActive(false);
                _panelTitle.GetComponent<RectTransform>().sizeDelta = _panelTitle.GetComponent<RectTransform>().sizeDelta.WithY(12);
            }
            else if (hasDescription)
            {
                _panelDescription.gameObject.SetActive(true);
                _panelDescription.text = description;
                _panelTitle.GetComponent<RectTransform>().sizeDelta = _panelTitle.GetComponent<RectTransform>().sizeDelta.WithY(8);
            }

            List<SectionLine> lines = [.. section.Lines];

            for (int i = 0; i < lines.Count; i++)
            {
                SectionLine line = lines[i];

                if (line.Options.HasFlag(LineOptions.Wrapping))
                {
                    int insertPosition = i;
                    lines.RemoveAt(insertPosition); // Remove the initial line that we're going to extend

                    string[] textArray = _sampleLineText.GetArrayFromText(line.Text);
                    for (int j = textArray.Length - 1; j >= 0; j--)
                    {
                        // Enumerates through the result of wrapping our initial line, effectively extending it
                        lines.Insert(insertPosition, new SectionLine(textArray[j], j == 0 ? line.Widgets : []));
                    }

                    i += textArray.Length - 1;
                }
            }

            for (int i = 0; i < _panelLines.Count; i++)
            {
                PanelLine panelLine = _panelLines[i];

                if (i >= Constants.SectionCapacity) Logging.Warning($"{i} >= {Constants.SectionCapacity}");

                if (lines.ElementAtOrDefault(i) is SectionLine screenLine)
                {
                    bool wasLineActive = panelLine.gameObject.activeSelf;
                    if (!wasLineActive) panelLine.gameObject.SetActive(true);

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
            Logging.Fatal($"Displaying screen contents of {_loadedScreen.GetType().Name} threw exception");
            Logging.Error(ex);
            PlayErrorSound();
        }
    }

    #endregion

    #region Registration utilities

    public bool RegisterScreen(Type type)
    {
        if (type == null) return false;

        Logging.Info($"RegisterScreen: {type.Name} of {type.Namespace}");

        if (_screens.ContainsKey(type))
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

        Component component = _screenRoot.AddComponent(type);

        if (component is InfoScreen screen)
        {
            if (_screens.ContainsValue(screen))
            {
                Logging.Warning("Registry contains value (component of type)");
                Destroy(component);
                return false;
            }

            screen.enabled = false;

            _screens[type] = screen;

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

        if (_shortcutRegistars.ContainsKey(type))
        {
            Logging.Warning("Registry contains key");
            return false;
        }

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

            _shortcutRegistars.TryAdd(type, registrar);
        }
        catch (Exception ex)
        {
            Logging.Fatal("CreateInstance exception");
            Logging.Error(ex);

            return false;
        }

        return true;
    }

    #endregion

    #region Plugin utilities

    public string GetPluginStateKey(PluginInfo plugin) => $"ModState_{plugin.Metadata.GUID}";

    public bool GetPersistentPluginState(PluginInfo plugin)
    {
        string key = GetPluginStateKey(plugin);
        return !DataManager.Instance.HasData(key) || DataManager.Instance.GetData(key, defaultValue: true, setDefaultValue: false);
    }

    public void SetPersistentPluginState(PluginInfo plugin, bool state)
    {
        string key = GetPluginStateKey(plugin);
        if (state && DataManager.Instance.HasData(key)) DataManager.Instance.RemoveData(key);
        else DataManager.Instance.SetData(key, state);
    }

    #endregion

    #region Methods

    public async void CheckVersion(Action<(bool isOutdated, string latestVersion)> result)
    {
        using UnityWebRequest request = UnityWebRequest.Get(Constants.Uri_LatestVersion);
        UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
        await asyncOperation;

        if (request.result > UnityWebRequest.Result.Success) return;

        Version installedVersion = new(Constants.Version);
        string latestVersionString = request.downloadHandler.text.Trim();
        result?.Invoke((Version.TryParse(latestVersionString, out Version latestVersion) && latestVersion > installedVersion, latestVersionString));
    }

    public bool CheckInteractionInterval(WatchInteractionSource source, float debounce, bool setInterval = true)
    {
        float time = Time.realtimeSinceStartup;

        if (!_interactionTime.TryGetValue(source, out float touchTime))
        {
            if (setInterval) _interactionTime.Add(source, time);
            return true;
        }

        if (time > (touchTime + debounce))
        {
            if (setInterval) _interactionTime[source] = time;
            return true;
        }

        return false;
    }

    public void SetInteractionInterval(WatchInteractionSource source)
    {
        float time = Time.realtimeSinceStartup;

        if (_interactionTime.ContainsKey(source)) _interactionTime[source] = time;
        else _interactionTime.Add(source, time);
    }

    public void PlayErrorSound()
    {
        string soundName = string.Concat("error", Random.Range(1, 7));
        if (Enum.TryParse(soundName, out Sounds sound)) _infoWatch.audioDevice.PlayOneShot(sound.AsAudioClip());
    }

    public void PressButton(PushButton button, bool isLeftHand)
    {
        if (button.Null()) return;
        AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(isLeftHand);
        handPlayer.PlayOneShot(button.TryGetComponent(out AudioSource component) ? component.clip : Sounds.widgetButton.AsAudioClip(), 0.2f);
    }

    public void PressSlider(SnapSlider slider, bool isLeftHand)
    {
        if (slider.Null()) return;
        AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(isLeftHand);
        handPlayer.PlayOneShot(Sounds.widgetSlider.AsAudioClip(), 0.2f);
    }

    public void PressSwitch(Switch button, bool isLeftHand)
    {
        if (button.Null()) return;
        AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(isLeftHand);
        handPlayer.PlayOneShot(button.TryGetComponent(out AudioSource component) ? component.clip : Sounds.widgetSwitch.AsAudioClip(), 0.2f);
    }

    #endregion

    #region Events

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Logging.Fatal($"OnJoinRoomFailed: {message} ({returnCode})");

        if (returnCode == ErrorCode.GameDoesNotExist || !Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Server)) return;
        Notifications.SendNotification(new("Room Join failure", returnCode == ErrorCode.GameFull ? "Room is full" : $"Code {returnCode}", 3, Sounds.notificationNegative));
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Logging.Fatal("OnCustomAuthenticationFailed");
        Logging.Message(debugMessage);

        if (!Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Server)) return;
        Notifications.SendNotification(new("Photon PUN failure", "Custom Auth failed", 3, Sounds.notificationNegative));
    }

    private void OnQuestCompleted(RotatingQuest quest)
    {
        Logging.Info($"Quest Completed: {quest.GetTextDescription()}");

        if (!Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Quest)) return;
        Notifications.SendNotification(new("You completed a Quest", quest.questName, 5, Sounds.notificationNeutral));
    }

    private void OnMothershipMessageRecieved(NotificationsMessageResponse notification, nint _)
    {
        string title = notification.Title;
        string body = notification.Body;

        Logging.Message($"Mothership message recieved:");
        Logging.Info("\"{title}\": \"{body}\"");

        if (!Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Server)) return;

        string[] array;

        try
        {
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
        catch
        {

        }
    }

    #endregion
}